namespace Sprout.Core.Services.Jobs
{
    public interface IJobScheduler : IDisposable
    {
        event EventHandler<JobStatusChangedEventArgs>? StatusChanged;

        void Start();
        void RefreshSchedules();
        JobRuntimeStatus GetStatus(Guid jobID);
        Task RunAsync(Guid jobID);
        void Stop(Guid jobID);
    }
}
