using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Factories
{
    public class DataAdapterFactory : IDataAdapterFactory
    {
        private readonly IConfigurationService _configurationService;

        public DataAdapterFactory(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public IDataAdapter Create(IDataAdapterConfig dataAdapterConfig)
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

        private IDataAdapter CreateSqlServerDataAdapter(SqlServerDataAdapterConfig sqlServerAdapterConfig)
        {
            var connectionString = string.IsNullOrWhiteSpace(sqlServerAdapterConfig.ConnectionString)
                ? _configurationService.Load().Settings.SqlServerConnectionString
                : sqlServerAdapterConfig.ConnectionString;

            var dataAdapter = new SqlServerDataAdapter
            {
                ConnectionString = connectionString
            };

            var dataProviderConfig = sqlServerAdapterConfig.DataProvider as SqlServerDataProviderConfig;

            if (dataProviderConfig is null)
                throw new Exception($"DataAdapter DataProvider type missmatch! Expected DataProvider is {nameof(SqlServerDataProviderConfig)}");

            var dataProvider = new SqlServerDataProvider(dataAdapter)
            {
                Text = dataProviderConfig.Text
            };

            dataAdapter.DataProvider = dataProvider;

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
                var deleteCommandConfig = sqlServerAdapterConfig.DeleteCommand as SqlServerEditCommandConfig;

                dataAdapter.DeleteCommand = new SqlServerEditCommand(dataAdapter)
                {
                    Text = deleteCommandConfig.Text
                };
            }

            if (dataProviderConfig.FilterConfigs.Count > 0)
            {
                foreach (var filterConfig in dataProviderConfig.FilterConfigs)
                {
                    dataProvider.Filters[filterConfig.Title] = new SqlServerFilter
                    {
                        Title = filterConfig.Title,
                        Text = filterConfig.Text,
                    };
                }
            }

            return dataAdapter;
        }
    }
}

