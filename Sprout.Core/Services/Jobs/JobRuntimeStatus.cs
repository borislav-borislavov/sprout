namespace Sprout.Core.Services.Jobs
{
    public enum JobRunState
    {
        Idle,
        Scheduled,
        Running,
        Stopping,
        Failed
    }

    public sealed record JobRuntimeStatus(
        Guid JobID,
        JobRunState State,
        DateTimeOffset? LastStartedAt,
        DateTimeOffset? LastCompletedAt,
        DateTimeOffset? NextRunAt,
        string? LastError);

    public sealed class JobStatusChangedEventArgs(JobRuntimeStatus status) : EventArgs
    {
        public JobRuntimeStatus Status { get; } = status;
    }
}
