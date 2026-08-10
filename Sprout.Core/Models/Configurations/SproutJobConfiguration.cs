namespace Sprout.Core.Models.Configurations
{
    public class SproutJobConfiguration
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "New job";
        public string Script { get; set; } = string.Empty;
        public List<string> Usings { get; set; } = [];
        public bool IsScheduleEnabled { get; set; }
        public string CronExpression { get; set; } = "0 0/5 * * * ?";
    }
}
