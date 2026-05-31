using Sprout.Core.Services.Configurations;
using System;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sprout.Core.Services.Logging
{
    /// <summary>
    /// Writes executed SQL queries to a file on disk when the
    /// <see cref="Models.Configurations.SproutSettings.LogSqlQueries"/> setting is enabled.
    /// </summary>
    public class FileSqlQueryLogger : ISqlQueryLogger
    {
        private const string LogFileName = "sql-queries.log";
        private const string RolledLogFileName = "sql-queries_1.log";
        private const long MaxLogFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly object _sync = new();

        private readonly IConfigurationService _configurationService;

        public FileSqlQueryLogger(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public void Log(string source, string commandText, DbParameterCollection parameters = null, TimeSpan duration = default)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                return;

            if (!_configurationService.Load().Settings.LogSqlQueries)
                return;

            try
            {
                var executableQuery = InlineParameters(commandText, parameters);

                var builder = new StringBuilder();
                builder.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(']');

                if (!string.IsNullOrWhiteSpace(source))
                    builder.Append(" [").Append(source).Append(']');

                if (duration != default)
                    builder.Append(" [").Append(duration.TotalMilliseconds.ToString("F2")).Append(" ms]");

                builder.AppendLine();
                builder.AppendLine(executableQuery.Trim());
                builder.AppendLine(new string('-', 60));

                lock (_sync)
                {
                    RollOverIfNeeded();
                    File.AppendAllText(GetLogFilePath(), builder.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // Logging must never break query execution.
            }
        }

        /// <summary>
        /// Replaces each named parameter placeholder in <paramref name="commandText"/> with
        /// its quoted/literal value so the result is directly executable.
        /// </summary>
        private static string InlineParameters(string commandText, DbParameterCollection parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return commandText;

            var result = commandText;

            foreach (DbParameter p in parameters)
            {
                var placeholder = p.ParameterName.StartsWith('@') || p.ParameterName.StartsWith('$') || p.ParameterName.StartsWith(':')
                    ? p.ParameterName
                    : "@" + p.ParameterName;

                var literal = p.Value == null || p.Value == DBNull.Value
                    ? "NULL"
                    : p.Value is string || p.Value is DateTime || p.Value is Guid
                        ? $"'{p.Value.ToString()!.Replace("'", "''")}'"
                        : p.Value.ToString();

                result = Regex.Replace(result, Regex.Escape(placeholder) + @"\b", literal, RegexOptions.IgnoreCase);
            }

            return result;
        }

        private static void RollOverIfNeeded()
        {
            var logPath = GetLogFilePath();

            if (!File.Exists(logPath))
                return;

            if (new FileInfo(logPath).Length < MaxLogFileSizeBytes)
                return;

            var rolledPath = Path.Combine(Environment.CurrentDirectory, RolledLogFileName);

            if (File.Exists(rolledPath))
                File.Delete(rolledPath);

            File.Move(logPath, rolledPath);
        }

        private static string GetLogFilePath()
        {
            return Path.Combine(Environment.CurrentDirectory, LogFileName);
        }
    }
}
