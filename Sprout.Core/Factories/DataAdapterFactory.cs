using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Factories
{
    public class DataAdapterFactory
    {
        public static IDataAdapter Create(IDataAdapterConfig dataAdapterConfig)
        {
            if (dataAdapterConfig is SqlServerDataAdapterConfig sqlServerAdapterConfig)
            {
                return CreateSqlServerDataAdapter(sqlServerAdapterConfig);
            }
            else
            {
                throw new NotImplementedException($"DataAdapterFactory does not support creating IDataAdapter for type {dataAdapterConfig.GetType().FullName}");
            }
        }

        private static IDataAdapter CreateSqlServerDataAdapter(SqlServerDataAdapterConfig sqlServerAdapterConfig)
        {
            var dataAdapter = new SqlServerDataAdapter
            {
                ConnectionString = sqlServerAdapterConfig.ConnectionString
            };

            var dataProviderConfig = sqlServerAdapterConfig.DataProvider as SqlServerDataProviderConfig;

            if (dataProviderConfig is null)
                throw new Exception($"DataAdapter DataProvider type missmatch! Expected DataProvider is {nameof(SqlServerDataProviderConfig)}");

            dataAdapter.DataProvider = new SqlServerDataProvider(dataAdapter)
            {
                Text = dataProviderConfig.Text
            };

#warning polish this
            (dataAdapter.DataProvider as SqlServerDataProvider).Dependencies = ParameterParser.ParseDependencies(dataProviderConfig.Text);

            if (sqlServerAdapterConfig.InsertCommand != null)
            {
                var insertCommandConfig = sqlServerAdapterConfig.InsertCommand as SqlServerEditCommandConfig;

                if (insertCommandConfig is null)
                    throw new Exception($"Expected InsertCommand should be of type {nameof(SqlServerEditCommandConfig)}");

                dataAdapter.InsertCommand = new SqlServerEditCommand(dataAdapter)
                {
                    Text = insertCommandConfig.Text
                };
            }

            if (sqlServerAdapterConfig.UpdateCommand != null)
            {
                var updateCommandConfig = sqlServerAdapterConfig.UpdateCommand as SqlServerEditCommandConfig;

                if (updateCommandConfig is null)
                    throw new Exception($"Expected UpdateCommand should be of type {nameof(SqlServerEditCommandConfig)}");

                dataAdapter.UpdateCommand = new SqlServerEditCommand(dataAdapter)
                {
                    Text = updateCommandConfig.Text
                };
            }

            if (sqlServerAdapterConfig.DeleteCommand != null)
            {
                throw new NotImplementedException("DeleteCommand creation is not implemented yet in DataAdapterFactory.");
            }

            return dataAdapter;
        }
    }
}
