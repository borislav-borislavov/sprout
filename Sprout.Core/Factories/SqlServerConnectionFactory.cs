using Microsoft.Data.SqlClient;
using Sprout.Core.Common;

namespace Sprout.Core.Factories
{
    public class SqlServerConnectionFactory
    {
        public static SqlConnection Create(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                ApplicationName = Const.AppName
            };

            return new SqlConnection(builder.ConnectionString);
        }
    }
}
