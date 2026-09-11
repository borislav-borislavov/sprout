namespace Sprout.Core.Features.SeedFileUpdateFeature
{
    public interface ISeedUpdaterFactory
    {
        ISeedUpdater Create(ISeedUpdateConfig updateConfig);
    }
}