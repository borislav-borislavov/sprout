namespace Sprout.Core.Services.Migration
{
    public class MigrationRecord
    {
        public string RelativePath { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
    }
}
