using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.Configurations.Api;
using System;

namespace Sprout.Core.ViewModels
{
    public partial class ApiDataAdapterVM : ObservableObject
    {
        [ObservableProperty]
        private ApiDataAdapterConfig _dataAdapter;

        /// <summary>
        /// Strongly-typed convenience accessor for the data provider.
        /// </summary>
        public ApiDataProviderConfig Provider => DataAdapter?.DataProvider as ApiDataProviderConfig;

        public HttpVerb[] VerbOptions { get; } = (HttpVerb[])Enum.GetValues(typeof(HttpVerb));

        public ApiDataAdapterVM(ApiDataAdapterConfig dataAdapter)
        {
            DataAdapter = dataAdapter;
        }
    }
}
