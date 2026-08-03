using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Features.Dependency;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.SproutControlVMs;

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

                var dep = ParseDependency(scope);
                if (dep == null) continue;

                dependencies.Add(dep);
            }

            return dependencies;
        }

        public static DataProviderDependency ParseDependency(string text)
        {
            var dependency = new DataProviderDependency();

            dependency.RawDependency = text;

            var chunks = text.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            //reactive dependencies must start with @, otherwise its a string literal
            if (chunks == null || chunks.Length == 0 || chunks[0][0] != '@')
            {
                return null;
            }

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

        /// <summary>
        /// Resolve all dependencies from a text and return it
        /// </summary>
        /// <param name="text"></param>
        /// <param name="vmRegistry"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string? ResolveDependencies(this string text, VMRegistry vmRegistry, bool strict = true)
        {
            if (strict && string.IsNullOrEmpty(text))
            {
                throw new Exception($"Dependency cannot be empty: {text}");
            }
            else if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var deps = ParseDependencies(text);

            if (strict && !deps.Any())
            {
                throw new Exception($"Dependency {text} is not found!");
            }

            foreach (var dep in deps)
            {
                var value = dep.GetValue(vmRegistry, strict);

                text = text.Replace($"{{{dep.RawDependency}}}", $"{value}");
            }

            return text;
        }

        public static object GetValue(this DataProviderDependency dep, VMRegistry vmRegistry, bool strict = true)
        {
            var vm = vmRegistry.Get(dep.ControlName);

            if (strict && vm == null)
            {
                throw new Exception($"Invalid dependency: {dep.RawDependency}");
            }

            return BindingEvaluator.Evaluate(vm, dep.PropertyPath);
        }
    }
}
