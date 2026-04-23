namespace Sprout.Core.Services.Migration
{
    public record MigrationError(string RelativeFilePath, int BatchNumber, string BatchContent, Exception ex);

    public class MigrationResult
    {
        public IReadOnlyList<MigrationFile> Executed { get; init; } = [];
        public MigrationError? Error { get; set; }

        public Exception? Exception { get; set; }
    }
}
