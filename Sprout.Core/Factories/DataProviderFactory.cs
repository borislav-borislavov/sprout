using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.Queries;
using Sprout.Core.Models.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Factories
{
    public class DataProviderFactory
    {
        public static IDataProvider Create(IDataProviderConfig dataProviderConfig)
        {
            if (dataProviderConfig is QueryConfig queryConfig)
            {
                return CreateSqlServerQuery(queryConfig);
            }
            else
            {
                throw new NotImplementedException($"DataProviderFactory does not support creating IDataProvider for type {dataProviderConfig.GetType().FullName}");
            }
        }

        private static IDataProvider CreateSqlServerQuery(QueryConfig queryConfig)
        {
            var query = new Query
            {
                Name = queryConfig.ProviderName,
                Text = queryConfig.Text,
                TableName = queryConfig.TableName,
                ConnectionString = queryConfig.ConnectionString,
            };

            query.Dependencies = ParameterParser.ParseDependencies(query.Text);

            AssignCommandIfAvailable(queryConfig.InsertCommand, query.InsertCommand);
            AssignCommandIfAvailable(queryConfig.UpdateCommand, query.UpdateCommand);
            AssignCommandIfAvailable(queryConfig.DeleteCommand, query.DeleteCommand);

            return query;

            void AssignCommandIfAvailable(TableOperationCommand tableOperationCommand,
                QueryCommand queryCommand)
            {
                if (tableOperationCommand == null) return;

                queryCommand.Text = tableOperationCommand.Text;
                queryCommand.DefaultValues = tableOperationCommand.DefaultValues;
            }
        }
    }
}
