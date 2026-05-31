using Sprout.Core.Models.Configurations.DataGrid;

namespace Sprout.Core.Models.Configurations
{
    public class SproutSettings
    {
        public string SqlServerConnectionString { get; set; } = string.Empty;
        public int CommandTimeout { get; set; } = 30;
        public string LastUsername { get; set; } = string.Empty;
        public string MigrationsFolder { get; set; } = AppContext.BaseDirectory;
        public string DuckDbConnectionString { get; set; } = string.Empty;
        public bool LogSqlQueries { get; set; }

        /// <summary>
        /// Persisted column layouts (visibility, order and frozen count) per SproutDataGrid, keyed by the grid name.
        /// </summary>
        public Dictionary<string, SproutGridColumnLayout> GridColumnLayouts { get; set; } = [];
    }
}
