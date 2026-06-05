using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters.Filters
{
    public class ApiFilter : IFilter
    {
        public string Title { get; set; }

        /// <summary>
        /// The query parameter name. Falls back to <see cref="Title"/> when empty.
        /// </summary>
        public string Text { get; set; }

        public object StartValue { get; set; }

        public object EndValue { get; set; }

        public bool IsRange { get; set; }

        /// <summary>
        /// Returns the effective query-parameter name used when building the request URL.
        /// </summary>
        public string QueryParamName => string.IsNullOrWhiteSpace(Text) ? Title : Text;
    }
}
