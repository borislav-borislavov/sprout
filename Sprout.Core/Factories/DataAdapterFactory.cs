using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.Duck;
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
            IDataAdapter dataAdapter;

            if (dataAdapterConfig is SqlServerDataAdapterConfig sqlServerAdapterConfig)
            {
                dataAdapter = CreateSqlServerDataAdapter(sqlServerAdapterConfig);
            }
            else if (dataAdapterConfig is DuckDataAdapterConfig duckAdapterConfig)
            {
                dataAdapter = CreateDuckDataAdapter(duckAdapterConfig);
            }
            else
            {
                throw new NotImplementedException($"DataAdapterFactory does not support creating IDataAdapter for type {dataAdapterConfig.GetType().FullName}");
            }

            dataAdapter.ParentType = dataAdapterConfig.ParentType;
            dataAdapter.Name = dataAdapterConfig.Name;

            return dataAdapter;
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

        private IDataAdapter CreateDuckDataAdapter(DuckDataAdapterConfig duckAdapterConfig)
        {
            var connectionString = duckAdapterConfig.ConnectionString;

            var dataAdapter = new DuckDataAdapter
            {
                ConnectionString = connectionString
            };

            var dataProviderConfig = duckAdapterConfig.DataProvider as DuckDataProviderConfig;

            if (dataProviderConfig is null)
                throw new Exception($"DataAdapter DataProvider type mismatch! Expected DataProvider is {nameof(DuckDataProviderConfig)}");

            var dataProvider = new DuckDataProvider(dataAdapter)
            {
                Text = dataProviderConfig.Text
            };

            dataAdapter.DataProvider = dataProvider;

            if (duckAdapterConfig.InsertCommand != null)
            {
                var insertCommandConfig = duckAdapterConfig.InsertCommand as DuckEditCommandConfig;

                if (insertCommandConfig is null)
                    throw new Exception($"Expected InsertCommand should be of type {nameof(DuckEditCommandConfig)}");

                dataAdapter.InsertCommand = new DuckEditCommand(dataAdapter)
                {
                    Text = insertCommandConfig.Text
                };
            }

            if (duckAdapterConfig.UpdateCommand != null)
            {
                var updateCommandConfig = duckAdapterConfig.UpdateCommand as DuckEditCommandConfig;

                if (updateCommandConfig is null)
                    throw new Exception($"Expected UpdateCommand should be of type {nameof(DuckEditCommandConfig)}");

                dataAdapter.UpdateCommand = new DuckEditCommand(dataAdapter)
                {
                    Text = updateCommandConfig.Text
                };
            }

            if (duckAdapterConfig.DeleteCommand != null)
            {
                var deleteCommandConfig = duckAdapterConfig.DeleteCommand as DuckEditCommandConfig;

                if (deleteCommandConfig is null)
                    throw new Exception($"Expected DeleteCommand should be of type {nameof(DuckEditCommandConfig)}");

                dataAdapter.DeleteCommand = new DuckEditCommand(dataAdapter)
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

