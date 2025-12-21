using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sprout.Core.Services.Queries.QueryService;

namespace Sprout.Core.Models.Queries
{
    public static class ParameterParser
    {
        public static IEnumerable<QueryParameter> ParseQueryParameters(string text)
        {
            List<QueryParameter> parameters = new();

            foreach (var scope in GetScopes(text))
            {
                parameters.Add(new QueryParameter
                {
                    Name = scope.TrimStart('@').TrimEnd('?', '!'),
                    IsMandatory = scope.EndsWith("!"),
                    RawPatameter = scope
                });
            }

            return parameters;
        }

        public static IEnumerable<QueryDependency> ParseDependencies(string text)
        {
            List<QueryDependency> dependencies = new();

            foreach (var scope in GetScopes(text))
            {
                var periodIdx = scope.IndexOf('.');
                if (periodIdx == -1) continue;

                var dependency = new QueryDependency();

                dependency.RawDependency = scope;

                var chunks = scope.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                dependency.ControlName = chunks[0].TrimStart('@');

                dependency.PropertyName = chunks[1];

                if (chunks.Length > 2)
                    dependency.Extra = chunks[2..];
                else
                    dependency.Extra = Array.Empty<string>();

                dependencies.Add(dependency);
            }

            return dependencies;
        }

        public static IEnumerable<string> GetScopes(this string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;

            int startIndex = 0;
            Scope scope = text.NextScope(startIndex: startIndex);

            while (scope != null)
            {
                startIndex = scope.CloseIdx;

                yield return scope.Content;

                scope = text.NextScope(startIndex: startIndex);
            }
        }
    }
}
