using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Models.DataAdapters.DataProviders;

namespace Sprout.Core.Models.Queries
{
    public static class DependencyParser
    {
        public static IEnumerable<DependencyMeta> ParseDependencyMetas(string text)
        {
            List<DependencyMeta> parameters = new();

            foreach (var scope in GetScopes(text))
            {
                var nrNavigations = scope.Count(c => c == '.');

                parameters.Add(new DependencyMeta
                {
                    Name = scope.TrimStart('@').TrimEnd('?', '!'),
                    IsMandatory = scope.EndsWith("!"),
                    RawPatameter = scope,

                    //Changed from 1 to 0 because RowParameters are accessed simply by their names
                    //And now when I added SproutGridUIState.JsonData it was not fetching it properly (tried to get it from the DataRow)
                    IsFromUIState = nrNavigations > 0
                });
            }

            return parameters;
        }

        public static IEnumerable<DataProviderDependency> ParseDependencies(string text)
        {
            List<DataProviderDependency> dependencies = new();

            foreach (var scope in GetScopes(text))
            {
                var periodIdx = scope.IndexOf('.');
                if (periodIdx == -1) continue;

                dependencies.Add(ParseDependency(scope));
            }

            return dependencies;
        }

        public static DataProviderDependency ParseDependency(string text)
        {
            var dependency = new DataProviderDependency();

            dependency.RawDependency = text;

            var chunks = text.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            dependency.ControlName = chunks[0].TrimStart('@');
            dependency.PropertyPath = string.Join(".", chunks[1..]);

            return dependency;
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
