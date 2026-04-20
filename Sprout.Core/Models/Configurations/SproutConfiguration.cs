namespace Sprout.Core.Models.Configurations
{
    public class SproutConfiguration
    {
        public LoginConfiguration? Login { get; set; }
        public List<SproutPageConfiguration> Pages { get; set; } = [];
        public SproutSettings Settings { get; set; } = new();
    }
}
