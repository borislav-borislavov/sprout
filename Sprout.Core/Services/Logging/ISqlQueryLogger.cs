using System.Data.Common;

namespace Sprout.Core.Services.Logging
{
    /// <summary>
    /// Logs executed SQL queries to a file when query logging is enabled in the settings.
    /// </summary>
    public interface ISqlQueryLogger
    {
        /// <summary>
        /// Appends the given SQL query (and its parameters) to the log file
        /// when the <c>LogSqlQueries</c> setting is enabled. Does nothing otherwise.
        /// </summary>
        void Log(string source, string commandText, DbParameterCollection parameters = null);
    }
}
