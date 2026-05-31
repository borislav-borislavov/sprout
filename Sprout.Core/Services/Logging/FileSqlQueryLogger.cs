using Sprout.Core.Services.Configurations;
using System;
using System.Data.Common;
using System.IO;
using System.Text;

namespace Sprout.Core.Services.Logging
{
    /// <summary>
    /// Writes executed SQL queries to a file on disk when the
    /// <see cref="Models.Configurations.SproutSettings.LogSqlQueries"/> setting is enabled.
    /// </summary>
    public class FileSqlQueryLogger : ISqlQueryLogger
    {
        private const string LogFileName = "sql-queries.log";

        private static readonly object _sync = new();

        private readonly IConfigurationService _configurationService;

        public FileSqlQueryLogger(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public void Log(string source, string commandText, DbParameterCollection parameters = null)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                return;

            if (!_configurationService.Load().Settings.LogSqlQueries)
                return;

            try
            {
                var builder = new StringBuilder();
                builder.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(']');

                if (!string.IsNullOrWhiteSpace(source))
                    builder.Append(" [").Append(source).Append(']');

                builder.AppendLine();
                builder.AppendLine(commandText.Trim());

                if (parameters != null && parameters.Count > 0)
                {
                    builder.AppendLine("Parameters:");

                    foreach (DbParameter parameter in parameters)
                    {
                        builder.Append("  ")
                               .Append(parameter.ParameterName)
                               .Append(" = ")
                               .AppendLine(parameter.Value?.ToString() ?? "NULL");
                    }
                }

                builder.AppendLine(new string('-', 60));

                lock (_sync)
                {
                    File.AppendAllText(GetLogFilePath(), builder.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // Logging must never break query execution.
            }
        }

        private static string GetLogFilePath()
        {
            return Path.Combine(Environment.CurrentDirectory, LogFileName);
        }
    }
}
