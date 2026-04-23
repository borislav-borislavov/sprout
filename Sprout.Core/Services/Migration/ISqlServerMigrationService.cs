namespace Sprout.Core.Services.Migration
{
    public interface ISqlServerMigrationService
    {
        Task<MigrationResult> RunMigrationsAsync();
    }
}