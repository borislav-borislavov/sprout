using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Factories
{
    public class DataServiceFactory : IDataServiceFactory
    {
        public IDataService Create(IDataAdapter dataAdapter, UiStateRegistry uiStateRegistry)
        {
            if (dataAdapter is SqlServerDataAdapter sqlServerDataAdapter)
            {
                return new SqlServerDataService(sqlServerDataAdapter, uiStateRegistry);
            }

            throw new NotSupportedException($"Data adapter of type {dataAdapter.GetType().Name} is not supported.");
        }
    }
}
