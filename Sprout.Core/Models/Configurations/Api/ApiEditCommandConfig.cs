using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.Api
{
    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public class ApiEditCommandConfig : IEditCommandConfig
    {
        /// <summary>
        /// The URL to call for this command. Supports {parameter} placeholders.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The HTTP verb to use when calling this command.
        /// </summary>
        public HttpVerb Verb { get; set; } = HttpVerb.POST;

        /// <summary>
        /// Optional request body template. When set, {ColumnName} placeholders are replaced
        /// with values from the current DataRow and {@Control.Property} placeholders are
        /// resolved from the UI state registry. When empty, all DataRow columns are
        /// automatically serialised as a JSON object.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// When true, a value extracted from the response is surfaced as an Info message
        /// in the UI after a successful call.
        /// </summary>
        public bool ShowResponseAsMessage { get; set; }

        /// <summary>
        /// Optional dot-notation or JSONPath expression pointing to the value inside the
        /// response that should be shown as the Info message (e.g. "message" or "$.data.text").
        /// When empty the entire response body is used.
        /// Only evaluated when <see cref="ShowResponseAsMessage"/> is true.
        /// </summary>
        public string ResponsePath { get; set; }
    }
}
