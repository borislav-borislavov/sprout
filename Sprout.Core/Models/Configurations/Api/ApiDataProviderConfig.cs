using System.Collections.ObjectModel;

namespace Sprout.Core.Models.Configurations.Api
{
    public class ApiDataProviderConfig : IDataProviderConfig
    {
        /// <summary>
        /// The URL template for the GET request. Supports {parameter} placeholders for dependencies.
        /// Filters are appended as query parameters.
        /// </summary>
        public string Text { get; set; }

        public ObservableCollection<FilterConfig> FilterConfigs { get; set; } = [];

        /// <summary>
        /// Optional dot-notation or JSONPath expression to navigate to the results array
        /// inside the API response (e.g. "data.results" or "$.data.results").
        /// Leave empty when the root of the response is already an array.
        /// </summary>
        public string DataPath { get; set; }

        /// <summary>
        /// Optional URL used to obtain a bearer token before the data request.
        /// When set, <see cref="AuthBody"/> is posted to this URL and the token
        /// from the response is attached as a Bearer Authorization header.
        /// </summary>
        public string AuthUrl { get; set; }

        /// <summary>
        /// JSON body template to POST to <see cref="AuthUrl"/>. 
        /// Supports {parameter} placeholders (e.g. {"username":"{user}","password":"{pass}"}).
        /// </summary>
        public string AuthBody { get; set; }

        public bool DeferInitialLoad { get; set; } = false;
    }
}
