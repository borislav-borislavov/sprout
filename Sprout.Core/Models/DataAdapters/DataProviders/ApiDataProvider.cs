using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.DataAdapters.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters.DataProviders
{
    public partial class ApiDataProvider : ObservableObject, IDataProvider
    {
        public string Text { get; set; }

        /// <summary>
        /// Dot-notation or JSONPath expression pointing to the results array in the response.
        /// </summary>
        public string DataPath { get; set; }

        /// <summary>
        /// URL used to obtain a bearer token prior to data requests.
        /// </summary>
        public string AuthUrl { get; set; }

        /// <summary>
        /// JSON body template POSTed to <see cref="AuthUrl"/>.
        /// </summary>
        public string AuthBody { get; set; }

        [ObservableProperty]
        private DataTable _data;

        public Dictionary<string, IFilter> Filters { get; set; } = [];

        public IEnumerable<DataProviderDependency> Dependencies { get; internal set; } = [];

        public bool DeferInitialLoad { get; set; } = false;

        private ApiDataAdapter _parentAdapter;

        public ApiDataProvider(ApiDataAdapter parentAdapter)
        {
            _parentAdapter = parentAdapter;
        }

        public IDataAdapter Parent => _parentAdapter;
    }
}
