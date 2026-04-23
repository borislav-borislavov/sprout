using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.SqlServer;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using static Sprout.Core.Common.Const;

namespace Sprout.Core.Services.Migration
{
    public class SqlServerMigrationService : ISqlServerMigrationService
    {
        private static readonly string SqlServerSubfolder = Path.Combine("MigrationsFolder", "SqlServer");
        private static readonly Regex GoBatchSplitter = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private readonly SqlServerService _sqlServer;
        private readonly SproutSettings _settings;

        public SqlServerMigrationService(IConfigurationService configurationService)
        {
            _settings = configurationService.Load().Settings;
            _sqlServer = new SqlServerService(_settings.SqlServerConnectionString);
        }

        /// <summary>
        /// Ensures the [sprout] schema and the [sprout].[_Migrations] table exist in the database,
        /// creating them if they are missing.
        /// </summary>
        private async Task EnsureSchemaAndTableAsync()
        {
            await EnsureSchemaAsync();
            await EnsureTableAsync();
        }

        private Task EnsureSchemaAsync() => _sqlServer.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'sprout')
            BEGIN
                EXEC('CREATE SCHEMA [sprout]')
            END
            """);

        private Task EnsureTableAsync() => _sqlServer.ExecuteAsync("""
            IF NOT EXISTS (
                SELECT 1
                FROM   sys.tables  t
                JOIN   sys.schemas s ON s.schema_id = t.schema_id
                WHERE  s.name = 'sprout' AND t.name = '_Migrations'
            )
            BEGIN
                CREATE TABLE [sprout].[_Migrations] (
                    RelativePath   NVARCHAR(255) PRIMARY KEY,
                    FileHash   NVARCHAR(64)  NOT NULL,
                    ExecutedAt DATETIME      NOT NULL DEFAULT (GETDATE())
                )
            END
            """);

        public async Task<MigrationResult> RunMigrationsAsync()
        {
            try
            {
                await _sqlServer.OpenConnectionAsync();

                await EnsureSchemaAndTableAsync();

                EnsureMigrationsFolderExists();

                var records = await GetMigrationsAsync();
                var files = ScanMigrationFiles();
                var changedFiles = GetPendingMigrations(files, records);

                if (!changedFiles.Any())
                {
                    return new MigrationResult { Executed = [] };
                }

                var executed = new List<MigrationFile>();
                Stopwatch sw = new();

                foreach (var file in changedFiles)
                {
                    sw.Start();

                    await _sqlServer.BeginTransactionAsync();

                    var batch = string.Empty;

                    var content = await File.ReadAllTextAsync(file.FullPath);

                    var batches = GoBatchSplitter
                        .Split(content)
                        .Select(b => b.Trim())
                        .Where(b => !string.IsNullOrEmpty(b))
                        .ToList();

                    for (int i = 0; i < batches.Count; i++)
                    {
                        try
                        {
                            batch = batches[i];
                            await _sqlServer.ExecuteAsync(batch);
                        }
                        catch (Exception ex)
                        {
                            await _sqlServer.RollbackTransactionAsync();

                            await LogAsync($"ERROR {file.RelativePath} | Batch {i}: {batch} | Exception: {ex}");

                            return new()
                            {
                                Executed = executed,
                                Error = new(file.FullPath, i, batch, ex)
                            };
                        }
                    }

                    await SaveMigration(file);

                    await _sqlServer.CommitTransactionAsync();
                    sw.Stop();

                    executed.Add(file);

                    await LogAsync($"Executed {file.RelativePath} in {sw.Elapsed}");
                    sw.Reset();
                }

                return new MigrationResult { Executed = executed };
            }
            catch (Exception ex)
            {
                await LogAsync($"EXCEPTION: {ex}");
                return new() { Exception = ex };
            }
            finally
            {
                await _sqlServer.CloseConnectionAsync();
            }
        }

        private async Task SaveMigration(MigrationFile file)
        {
            var sql = $"""
                UPDATE [sprout].[_Migrations]
                SET
                    FileHash = @FileHash,
                    ExecutedAt = GETDATE()
                WHERE RelativePath = @RelativePath

                IF (@@ROWCOUNT = 0)
                BEGIN
                    INSERT INTO [sprout].[_Migrations] (RelativePath, FileHash)
                    VALUES (@RelativePath, @FileHash)
                END
                """;

            await _sqlServer.ExecuteAsync(sql, new { file.FileHash, file.RelativePath });
        }

        private void EnsureMigrationsFolderExists()
        {
            var folder = Path.Combine(_settings.MigrationsFolder, SqlServerSubfolder);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        private Task<List<MigrationRecord>> GetMigrationsAsync() =>
            _sqlServer.QueryAsync<MigrationRecord>("SELECT RelativePath, FileHash, ExecutedAt FROM [sprout].[_Migrations]");

        private IReadOnlyList<MigrationFile> ScanMigrationFiles()
        {
            var folder = Path.Combine(_settings.MigrationsFolder, SqlServerSubfolder);

            if (!Directory.Exists(folder))
                return [];

            return Directory
                .EnumerateFiles(folder, "*.sql", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path)
                .Select(path => new MigrationFile(Path.GetRelativePath(_settings.MigrationsFolder, path), path, ComputeFileHash(path)))
                .ToList();
        }

        private static string ComputeFileHash(string fullPath)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(fullPath));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private IReadOnlyList<MigrationFile> GetPendingMigrations(
            IReadOnlyList<MigrationFile> files,
            IReadOnlyList<MigrationRecord> records)
        {
            var executedByName = records.ToDictionary(r => r.RelativePath, StringComparer.OrdinalIgnoreCase);

            return files
                .Where(f => !executedByName.TryGetValue(f.RelativePath, out var record)
                         || record.FileHash != f.FileHash)
                .ToList();
        }

        private Task LogAsync(string message)
        {
            var logPath = Path.Combine(_settings.MigrationsFolder, "sprout_migrations.log");
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            return File.AppendAllTextAsync(logPath, line + Environment.NewLine);
        }
    }
}
