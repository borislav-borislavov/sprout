namespace Sprout.Core.Models.Configurations
{
    public class SproutConfiguration
    {
        public List<SproutPageConfiguration> Pages { get; set; } = [];
        public SproutSettings Settings { get; set; } = new();
    }
}
