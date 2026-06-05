using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.Api;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.Duck;
using Sprout.Core.Services.Logging;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Factories
{
    public class DataServiceFactory : IDataServiceFactory
    {
        private readonly ISqlQueryLogger _sqlQueryLogger;
        private readonly IHttpClientFactory _httpClientFactory;

        public DataServiceFactory(ISqlQueryLogger sqlQueryLogger, IHttpClientFactory httpClientFactory)
        {
            _sqlQueryLogger = sqlQueryLogger;
            _httpClientFactory = httpClientFactory;
        }

        public IDataService Create(IDataAdapter dataAdapter, UiStateRegistry uiStateRegistry)
        {
            if (dataAdapter is SqlServerDataAdapter sqlServerDataAdapter)
            {
                return new SqlServerDataService(sqlServerDataAdapter, uiStateRegistry, _sqlQueryLogger);
            }

            if (dataAdapter is DuckDataAdapter duckDataAdapter)
            {
                return new DuckDbDataService(duckDataAdapter, uiStateRegistry, _sqlQueryLogger);
            }

            if (dataAdapter is ApiDataAdapter apiDataAdapter)
            {
                return new ApiDataService(apiDataAdapter, uiStateRegistry, _httpClientFactory, _sqlQueryLogger);
            }

            throw new NotSupportedException($"Data adapter of type {dataAdapter.GetType().Name} is not supported.");
        }
    }
}
