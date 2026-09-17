namespace Sprout.Core.Features.SeedFileUpdateFeature
{
    public interface ISeedUpdater
    {
        Task TryUpdate();
        Task Publish();
    }
}
