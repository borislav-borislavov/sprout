using Sprout.Core.Common;
using Sprout.Core.Models.DataAdapters.DataProviders;

namespace Sprout.Core.Services
{
    public class QueryBuildResult
    {
        public string QueryText { get; set; }
        public List<QueryBuildParameter> Parameters { get; set; } = [];
    }

    public class QueryBuildParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public static class QueryBuilder
    {
        public static QueryBuildResult Build(string queryTemplate, IDataProvider dataProvider, string parameterPrefix = "@")
        {
            //A button with just an update action could not have a query
            if (string.IsNullOrWhiteSpace(queryTemplate))
                return new QueryBuildResult();

            string queryText = queryTemplate;
            var parameters = new List<QueryBuildParameter>();

            var idx = 0;

            foreach (var item in dataProvider.Dependencies)
            {
                var paramName = $"{parameterPrefix}depParam{idx}";
                queryText = queryText.Replace($"{{{item.RawDependency}}}", paramName);
                parameters.Add(new QueryBuildParameter { Name = paramName, Value = item.Value ?? DBNull.Value });
                idx++;
            }

            var filterStatements = new List<string>();

            if (dataProvider.Filters.Count > 0)
            {
                int filterIdx = 0;

                foreach (var filter in dataProvider.Filters.Values)
                {
                    if (string.IsNullOrEmpty($"{filter.StartValue}") && string.IsNullOrEmpty($"{filter.EndValue}"))
                    {
                        filterIdx++;
                        continue;
                    }

                    string filterStatement;

                    if (filter.IsRange)
                    {
                        var startParamName = $"{parameterPrefix}filter_{filterIdx}_Start{idx}";
                        parameters.Add(new QueryBuildParameter { Name = startParamName, Value = filter.StartValue ?? DBNull.Value });
                        idx++;

                        var endParamName = $"{parameterPrefix}filter_{filterIdx}_End{idx}";
                        parameters.Add(new QueryBuildParameter { Name = endParamName, Value = filter.EndValue ?? DBNull.Value });
                        idx++;

                        filterStatement = string.Format(filter.Text, startParamName, endParamName);
                    }
                    else
                    {
                        var paramName = $"{parameterPrefix}filter_{filterIdx}_{idx}";
                        parameters.Add(new QueryBuildParameter { Name = paramName, Value = filter.StartValue ?? DBNull.Value });
                        idx++;

                        filterStatement = string.Format(filter.Text, paramName);
                    }

                    filterStatements.Add(filterStatement);
                    filterIdx++;
                }
            }

            if (filterStatements.Count > 0
                && queryText.IndexOf(Const.SqlServer.WhereFilter, StringComparison.OrdinalIgnoreCase) == -1
                && queryText.IndexOf(Const.SqlServer.AndFilter, StringComparison.OrdinalIgnoreCase) == -1)
            {
                throw new Exception($"Filters are added but {Const.SqlServer.WhereFilter} or {Const.SqlServer.AndFilter} not used in query");
            }

            // Replace WhereFilter syntax
            if (queryText.IndexOf(Const.SqlServer.WhereFilter, StringComparison.OrdinalIgnoreCase) != -1)
            {
                var replacement = filterStatements.Count == 0
                    ? string.Empty
                    : $"WHERE {string.Join($"{Environment.NewLine}AND ", filterStatements)}";

                queryText = queryText.Replace(Const.SqlServer.WhereFilter, replacement, StringComparison.OrdinalIgnoreCase);
            }

            // Replace AndFilter syntax
            if (queryText.IndexOf(Const.SqlServer.AndFilter, StringComparison.OrdinalIgnoreCase) != -1)
            {
                var replacement = filterStatements.Count == 0
                    ? string.Empty
                    : $" AND {string.Join($"{Environment.NewLine}AND ", filterStatements)}";

                queryText = queryText.Replace(Const.SqlServer.AndFilter, replacement, StringComparison.OrdinalIgnoreCase);
            }

            return new QueryBuildResult { QueryText = queryText, Parameters = parameters };
        }
    }
}
