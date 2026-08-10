using Quartz;
using Quartz.Impl;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace Sprout.Core.Services.Jobs
{
    public sealed class JobScheduler(IConfigurationService configurationService) : IJobScheduler
    {
        private const string SchedulerContextKey = "SproutJobScheduler";

        private readonly ConcurrentDictionary<Guid, RuntimeEntry> _entries = [];
        private readonly CancellationTokenSource _shutdown = new();
        private IScheduler? _quartzScheduler;

        public event EventHandler<JobStatusChangedEventArgs>? StatusChanged;

        /// <summary>
        /// Initialize the JobScheduler
        /// </summary>
        public void Start()
        {
            if (_quartzScheduler is not null)
                return;

            var factory = new StdSchedulerFactory(new System.Collections.Specialized.NameValueCollection
            {
                ["quartz.scheduler.instanceName"] = "SproutJobs",
                ["quartz.threadPool.maxConcurrency"] = "5"
            });
            _quartzScheduler = factory.GetScheduler().GetAwaiter().GetResult();
            _quartzScheduler.Context.Put(SchedulerContextKey, this);
            _quartzScheduler.Start().GetAwaiter().GetResult();

            RefreshSchedules();
        }

        /// <summary>
        /// Start/Re-Start job schedules
        /// </summary>
        public void RefreshSchedules()
        {
            var jobs = configurationService.Load().Jobs;
            var jobIDs = jobs.Select(j => j.ID).ToHashSet();

            foreach (var removedID in _entries.Keys.Where(id => !jobIDs.Contains(id)))
            {
                if (_entries.TryRemove(removedID, out var removed))
                    removed.Cancellation?.Cancel();
                _quartzScheduler?.DeleteJob(GetJobKey(removedID)).GetAwaiter().GetResult();
            }

            foreach (var job in jobs)
            {
                var entry = _entries.GetOrAdd(job.ID, _ => new RuntimeEntry());
                var nextRunAt = SyncQuartzSchedule(job);

                lock (entry.SyncRoot)
                {
                    entry.ScheduleEnabled = job.IsScheduleEnabled;
                    entry.NextRunAt = nextRunAt;
                    if (entry.State is JobRunState.Idle or JobRunState.Scheduled)
                        entry.State = job.IsScheduleEnabled ? JobRunState.Scheduled : JobRunState.Idle;
                }
                Publish(job.ID, entry);
            }
        }

        private DateTimeOffset? SyncQuartzSchedule(SproutJobConfiguration job)
        {
            if (_quartzScheduler is null)
                return null;

            var jobKey = GetJobKey(job.ID);
            if (!job.IsScheduleEnabled || !CronExpression.IsValidExpression(job.CronExpression))
            {
                _quartzScheduler.DeleteJob(jobKey).GetAwaiter().GetResult();
                return null;
            }

            var jobDetail = JobBuilder.Create<QuartzJobProxy>()
                .WithIdentity(jobKey)
                .UsingJobData("jobID", job.ID.ToString("N"))
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"trigger-{job.ID:N}")
                .WithCronSchedule(job.CronExpression, c => c.WithMisfireHandlingInstructionDoNothing())
                .ForJob(jobKey)
                .Build();

            _quartzScheduler.DeleteJob(jobKey).GetAwaiter().GetResult();
            _quartzScheduler.ScheduleJob(jobDetail, trigger).GetAwaiter().GetResult();
            return trigger.GetNextFireTimeUtc()?.ToLocalTime();
        }

        private static JobKey GetJobKey(Guid jobID) => new($"job-{jobID:N}");

        [DisallowConcurrentExecution]
        private sealed class QuartzJobProxy : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                var scheduler = (JobScheduler)context.Scheduler.Context.Get(SchedulerContextKey)!;
                var jobID = Guid.ParseExact(context.MergedJobDataMap.GetString("jobID")!, "N");

                try
                {
                    await scheduler.RunAsync(jobID).ConfigureAwait(false);
                }
                catch
                {
                    // Failures are captured and published as job status; do not fault the Quartz trigger.
                }

                if (scheduler._entries.TryGetValue(jobID, out var entry))
                {
                    lock (entry.SyncRoot)
                        entry.NextRunAt = context.Trigger.GetNextFireTimeUtc()?.ToLocalTime();
                    scheduler.Publish(jobID, entry);
                }
            }
        }

        public JobRuntimeStatus GetStatus(Guid jobID)
        {
            var entry = _entries.GetOrAdd(jobID, _ => new RuntimeEntry());
            lock (entry.SyncRoot)
                return CreateStatus(jobID, entry);
        }

        public async Task RunAsync(Guid jobID)
        {
            var liveDebugJob = new Job();

            if (liveDebugJob.PageId == jobID)
            {
                liveDebugJob.ExecuteAsync(_shutdown.Token).GetAwaiter().GetResult();
                return;
            }

            var job = configurationService.Load().Jobs.FirstOrDefault(j => j.ID == jobID)
                ?? throw new InvalidOperationException("The requested job no longer exists.");
            var entry = _entries.GetOrAdd(jobID, _ => new RuntimeEntry());

            lock (entry.SyncRoot)
            {
                if (entry.Execution is { IsCompleted: false })
                    return;

                entry.Cancellation = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
                entry.State = JobRunState.Running;
                entry.LastStartedAt = DateTimeOffset.Now;
                entry.LastError = null;
                entry.Execution = Task.Run(() => ExecuteAsync(job, entry, entry.Cancellation.Token));
            }

            Publish(jobID, entry);
            await entry.Execution.ConfigureAwait(false);
        }

        public void Stop(Guid jobID)
        {
            if (!_entries.TryGetValue(jobID, out var entry))
                return;

            lock (entry.SyncRoot)
            {
                if (entry.Execution is not { IsCompleted: false })
                    return;

                entry.State = JobRunState.Stopping;
                entry.Cancellation?.Cancel();
            }
            Publish(jobID, entry);
        }

        private async Task ExecuteAsync(SproutJobConfiguration job, RuntimeEntry entry, CancellationToken cancellationToken)
        {
            PageLogicLoadContext? loadContext = null;
            try
            {
                var data = GetOrCompile(job, entry);
                loadContext = new PageLogicLoadContext(job.ID.ToString("N"));
                using var stream = new MemoryStream(data);
                var assembly = loadContext.LoadFromStream(stream);
                var jobType = assembly.GetType($"DynamicJob._{job.ID:N}.Job", throwOnError: true)!;
                var instance = (BaseSproutJob)Activator.CreateInstance(jobType)!;
                await instance.ExecuteAsync(cancellationToken).ConfigureAwait(false);

                lock (entry.SyncRoot)
                    entry.State = entry.ScheduleEnabled ? JobRunState.Scheduled : JobRunState.Idle;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                lock (entry.SyncRoot)
                    entry.State = entry.ScheduleEnabled ? JobRunState.Scheduled : JobRunState.Idle;
            }
            catch (Exception ex)
            {
                lock (entry.SyncRoot)
                {
                    entry.State = JobRunState.Failed;
                    entry.LastError = ex is TargetInvocationException { InnerException: not null }
                        ? ex.InnerException.ToString()
                        : ex.ToString();
                }
            }
            finally
            {
                lock (entry.SyncRoot)
                {
                    entry.LastCompletedAt = DateTimeOffset.Now;
                    entry.Cancellation?.Dispose();
                    entry.Cancellation = null;
                }
                loadContext?.Unload();
                Publish(job.ID, entry);
            }
        }

        private void Publish(Guid jobID, RuntimeEntry entry)
        {
            JobRuntimeStatus status;
            lock (entry.SyncRoot)
                status = CreateStatus(jobID, entry);
            StatusChanged?.Invoke(this, new JobStatusChangedEventArgs(status));
        }

        private static JobRuntimeStatus CreateStatus(Guid jobID, RuntimeEntry entry) =>
            new(jobID, entry.State, entry.LastStartedAt, entry.LastCompletedAt, entry.NextRunAt, entry.LastError);

        public void Dispose()
        {
            _shutdown.Cancel();
            foreach (var entry in _entries.Values)
                entry.Cancellation?.Cancel();
            _quartzScheduler?.Shutdown(waitForJobsToComplete: false).GetAwaiter().GetResult();
            _shutdown.Dispose();
        }

        private sealed class RuntimeEntry
        {
            public object SyncRoot { get; } = new();
            public JobRunState State { get; set; }
            public bool ScheduleEnabled { get; set; }
            public DateTimeOffset? LastStartedAt { get; set; }
            public DateTimeOffset? LastCompletedAt { get; set; }
            public DateTimeOffset? NextRunAt { get; set; }
            public string? LastError { get; set; }
            public CancellationTokenSource? Cancellation { get; set; }
            public Task? Execution { get; set; }
            public int CompiledScriptHash { get; set; }
            public byte[]? CompiledAssembly { get; set; }
        }

        private byte[] GetOrCompile(SproutJobConfiguration job, RuntimeEntry entry)
        {
            var hash = HashCode.Combine(job.Script, string.Join(";", job.Usings));
            lock (entry.SyncRoot)
            {
                if (entry.CompiledAssembly is not null && entry.CompiledScriptHash == hash)
                    return entry.CompiledAssembly;
            }

            var result = new JobCompiler(job, configurationService).Compile();
            if (!result.IsSuccess || result.Assembly is null)
                throw new InvalidOperationException(/* join error diagnostics as today */);

            lock (entry.SyncRoot)
            {
                entry.CompiledScriptHash = hash;
                entry.CompiledAssembly = result.Assembly;
            }
            return result.Assembly;
        }
    }
}
