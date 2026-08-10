using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Jobs;
using System.IO;

namespace Sprout.Tests.Jobs
{
    public class JobsTests
    {
        private sealed class InMemoryConfigurationService : IConfigurationService
        {
            private SproutConfiguration _configuration;

            public InMemoryConfigurationService(SproutConfiguration configuration)
            {
                _configuration = configuration;
            }

            public SproutConfiguration Load() => _configuration;

            public bool Save(SproutConfiguration sproutConfiguration)
            {
                _configuration = sproutConfiguration;
                return true;
            }
        }

        private static SproutJobConfiguration CreateJob(string script) => new()
        {
            Name = "Test job",
            Script = script
        };

        [Fact]
        public void JobCompiler_CompilesValidScript()
        {
            var job = CreateJob("""
                public override async Task ExecuteAsync(CancellationToken cancellationToken)
                {
                    await Task.CompletedTask;
                }
                """);
            var compiler = new JobCompiler(job, new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] }));

            var result = compiler.Compile();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Assembly);
        }

        [Fact]
        public void JobCompiler_ReportsDiagnosticsForInvalidScript()
        {
            var job = CreateJob("this is not valid C#");
            var compiler = new JobCompiler(job, new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] }));

            var result = compiler.Compile();

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Diagnostics, d => d.Severity == "Error");
        }

        [Fact]
        public void JobCompiler_SaveUserScript_PersistsScript()
        {
            var job = CreateJob("// original");
            var configService = new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] });
            var compiler = new JobCompiler(job, configService)
            {
                UserScript = "// updated"
            };

            compiler.SaveUserScript();

            Assert.Equal("// updated", configService.Load().Jobs.Single().Script);
        }

        [Fact]
        public async Task JobScheduler_RunAsync_ExecutesJobAndReportsCompletion()
        {
            var marker = Path.Combine(Path.GetTempPath(), $"sprout_job_{Guid.NewGuid():N}.txt");
            var job = CreateJob($$"""
                public override async Task ExecuteAsync(CancellationToken cancellationToken)
                {
                    await File.WriteAllTextAsync(@"{{marker}}", "ran", cancellationToken);
                }
                """);
            using var scheduler = new JobScheduler(new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] }));

            try
            {
                await scheduler.RunAsync(job.ID);

                var status = scheduler.GetStatus(job.ID);
                Assert.True(File.Exists(marker));
                Assert.Equal(JobRunState.Idle, status.State);
                Assert.Null(status.LastError);
                Assert.NotNull(status.LastStartedAt);
                Assert.NotNull(status.LastCompletedAt);
            }
            finally
            {
                File.Delete(marker);
            }
        }

        [Fact]
        public async Task JobScheduler_RunAsync_CapturesJobFailure()
        {
            var job = CreateJob("""
                public override Task ExecuteAsync(CancellationToken cancellationToken)
                {
                    throw new InvalidOperationException("boom");
                }
                """);
            using var scheduler = new JobScheduler(new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] }));

            await scheduler.RunAsync(job.ID);

            var status = scheduler.GetStatus(job.ID);
            Assert.Equal(JobRunState.Failed, status.State);
            Assert.Contains("boom", status.LastError);
        }

        [Fact]
        public async Task JobScheduler_Stop_CancelsRunningJob()
        {
            var job = CreateJob("""
                public override async Task ExecuteAsync(CancellationToken cancellationToken)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                }
                """);
            using var scheduler = new JobScheduler(new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] }));

            var run = scheduler.RunAsync(job.ID);

            // Wait for the job to reach the running state before stopping it.
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (scheduler.GetStatus(job.ID).State != JobRunState.Running && DateTime.UtcNow < deadline)
                await Task.Delay(50);
            Assert.Equal(JobRunState.Running, scheduler.GetStatus(job.ID).State);

            scheduler.Stop(job.ID);
            await run.WaitAsync(TimeSpan.FromSeconds(30));

            Assert.Equal(JobRunState.Idle, scheduler.GetStatus(job.ID).State);
        }

        [Fact]
        public void JobScheduler_RefreshSchedules_SetsNextRunForScheduledJobs()
        {
            var job = CreateJob("// noop");
            job.IsScheduleEnabled = true;
            job.CronExpression = "0 0/2 * * * ?";
            using var scheduler = new JobScheduler(new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] }));

            scheduler.Start();

            var status = scheduler.GetStatus(job.ID);
            Assert.Equal(JobRunState.Scheduled, status.State);
            Assert.NotNull(status.NextRunAt);
            Assert.True(status.NextRunAt > DateTimeOffset.Now);
        }

        [Fact]
        public void JobScheduler_RefreshSchedules_IgnoresInvalidCronExpression()
        {
            var job = CreateJob("// noop");
            job.IsScheduleEnabled = true;
            job.CronExpression = "not a cron";
            using var scheduler = new JobScheduler(new InMemoryConfigurationService(new SproutConfiguration { Jobs = [job] }));

            scheduler.Start();

            var status = scheduler.GetStatus(job.ID);
            Assert.Null(status.NextRunAt);
        }
    }
}
