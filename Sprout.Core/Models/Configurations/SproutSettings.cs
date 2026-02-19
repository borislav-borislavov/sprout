namespace Sprout.Core.Models.Configurations
{
    public class SproutSettings
    {
        public string SqlServerConnectionString { get; set; } = string.Empty;
        public int CommandTimeout { get; set; } = 30;
    }
}
