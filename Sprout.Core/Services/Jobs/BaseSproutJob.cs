namespace Sprout.Core.Services.Jobs
{
    public abstract class BaseSproutJob
    {
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
