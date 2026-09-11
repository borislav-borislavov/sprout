using Sprout.Core.Features.SeedFileUpdateFeature;

namespace Sprout.Core.Models.Configurations
{
    public class SproutConfiguration
    {
        public string Version { get; set; } = "0";
        public ISeedUpdateConfig? SeedUpdateConfig { get; set; }
        public LoginConfiguration? Login { get; set; }
        public List<SproutPageConfiguration> Pages { get; set; } = [];
        public List<SproutMenuCategory> Categories { get; set; } = [];
        public List<SproutJobConfiguration> Jobs { get; set; } = [];
        public SproutSettings Settings { get; set; } = new();
    }
}
