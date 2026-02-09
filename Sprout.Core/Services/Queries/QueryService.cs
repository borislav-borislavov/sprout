using Microsoft.Data.SqlClient;
using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters.DataProviders;

namespace Sprout.Core.Services.Queries
{
    public class QueryService
    {
        public static async Task ExecuteQuery(SqlServerDataProvider dataProvider)
        {
            #warning separate query building from query execution
            var queryText = dataProvider.Text;

            var dependencyParameters = new List<SqlParameter>();
            var idx = 0;

            foreach (var item in dataProvider.Dependencies)
            {
                var paramName = $"@depParam{idx}";
                queryText = queryText.Replace($"{{{item.RawDependency}}}", paramName);
                SqlParameter sqlParameter = new SqlParameter(paramName, item.Value ?? DBNull.Value);
                dependencyParameters.Add(sqlParameter);
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
                        continue;
                    }

                    var filterStatement = filter.Text;

                    if (filter.IsRange)
                    {
                        var startParamName = $"@filter_{filterIdx}_Start{idx}";
                        var sqlParameter = new SqlParameter(startParamName, filter.StartValue ?? DBNull.Value);
                        dependencyParameters.Add(sqlParameter);
                        idx++;

                        var endParamName = $"@filter_{filterIdx}_End{idx}";
                        sqlParameter = new SqlParameter(endParamName, filter.EndValue ?? DBNull.Value);
                        dependencyParameters.Add(sqlParameter);
                        idx++;

                        filterStatement = string.Format(filter.Text, startParamName, endParamName);
                    }
                    else
                    {
                        var paramName = $"@filter_{filterIdx}_{idx}";
                        var sqlParameter = new SqlParameter(paramName, filter.StartValue ?? DBNull.Value);
                        dependencyParameters.Add(sqlParameter);
                        idx++;

                        filterStatement = string.Format(filter.Text, paramName);
                    }

                    filterStatements.Add(filterStatement);

                    filterIdx++;
                }
            }

            if (filterStatements.Count > 0 && (queryText.IndexOf("{!whereFilter}") == -1 && queryText.IndexOf("{!andFilter}") == -1))
            {
                throw new Exception("Filters are added but {!whereFilter} or {!andFilter} not used in query");
            }

            //replace WhereFilter syntax
            if (queryText.IndexOf("{!whereFilter}") != -1)
            {
                if (filterStatements.Count == 0)
                {
                    queryText = queryText.Replace("{!whereFilter}", string.Empty, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    var whereClause = $"WHERE {string.Join($"{Environment.NewLine}AND ", filterStatements)}";
                    queryText = queryText.Replace("{!whereFilter}", whereClause, StringComparison.OrdinalIgnoreCase);
                }
            }

            //replace AndFilter syntax
            if (queryText.IndexOf("{!andFilter}") != -1)
            {
                if (filterStatements.Count == 0)
                {
                    queryText = queryText.Replace("{!andFilter}", string.Empty, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    var whereClause = $" AND {string.Join($"{Environment.NewLine}AND ", filterStatements)}";
                    queryText = queryText.Replace("{!andFilter}", whereClause, StringComparison.OrdinalIgnoreCase);
                }
            }

            using (var conn = new SqlConnection(dataProvider.ConnectionString))
            using (var cmd = new SqlCommand(queryText, conn))
            {
                cmd.Parameters.AddRange(dependencyParameters.ToArray());

                //prevents the UI from freezing if the connection is slow
                await conn.OpenAsync();

                var dt = DataTableFactory.Create();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // Move the CPU-heavy loading to a background thread
                    // This prevents the UI from freezing during the data parsing
                    await Task.Run(() => dt.Load(reader));

                    dataProvider.Data = dt;
                }
            }
        }

        public class QueryParameter
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public object Value { get; set; }
            public bool IsMandatory { get; set; }
            public string RawPatameter { get; internal set; }
        }
    }
}
