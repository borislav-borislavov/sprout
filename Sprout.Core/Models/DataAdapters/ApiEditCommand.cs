using Sprout.Core.Models.Configurations.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters
{
    public class ApiEditCommand : IEditCommand
    {
        private readonly ApiDataAdapter _parentAdapter;

        public string Text { get; set; }

        public HttpVerb Verb { get; set; }

        /// <summary>
        /// Optional request body template. Supports {ColumnName} and {@Control.Property}
        /// placeholders. When empty, all DataRow columns are serialised automatically.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// When true, a value from the response is surfaced as an Info message after success.
        /// </summary>
        public bool ShowResponseAsMessage { get; set; }

        /// <summary>
        /// Dot-notation or JSONPath into the response JSON for the message value.
        /// When empty the entire response body string is used.
        /// </summary>
        public string ResponsePath { get; set; }

        public ApiEditCommand(ApiDataAdapter parentAdapter)
        {
            _parentAdapter = parentAdapter;
        }
    }
}
