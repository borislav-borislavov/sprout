using Microsoft.Data.SqlClient;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.GridActions;
using System.Linq;

namespace Sprout.Core.Services.Queries
{
    public class QueryService
    {
        public static void ExecuteQuery(SqlServerDataProvider dataProvider)
        {
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
                //TODO: implement filtering
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

                using (var da = new SqlDataAdapter(cmd))
                {
                    //cmd.Parameters.AddRange(AddDependencyParameters(query).ToArray());
                    //if (parameters != null && parameters.Length > 0)
                    //    cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    dataProvider.Data.Clear();
                    da.Fill(dataProvider.Data);
                }
            }

        }

        //#warning move this to a factory for consistency with the design
        //        public static Query CreateQuery(QueryConfig queryConfig)
        //        {
        //            ValidateQueryConfig(queryConfig);

        //            var query = new Query
        //            {
        //                Name = queryConfig.ProviderName,
        //                Text = queryConfig.Text,
        //                TableName = queryConfig.TableName,
        //                ConnectionString = queryConfig.ConnectionString
        //            };

        //            query.Dependencies = ParameterParser.ParseDependencies(query.Text);

        //            AssignCommandIfAvailable(queryConfig.InsertCommand, query.InsertCommand);
        //            AssignCommandIfAvailable(queryConfig.UpdateCommand, query.UpdateCommand);
        //            AssignCommandIfAvailable(queryConfig.DeleteCommand, query.DeleteCommand);

        //            return query;
        //        }

        //public static void BindDependencies(Query query, UiStateRegistry uiStateRegistry)
        //{
        //    foreach (var dep in query.Dependencies)
        //    {
        //        BindingOperations.SetBinding(
        //            target: dep,
        //            DataProviderDependency.ValueProperty,
        //            new Binding
        //            {
        //                Source = uiStateRegistry,
        //                Path = new PropertyPath($"[{dep.ControlName}].{dep.PropertyPath}")
        //            });
        //    }
        //}

        //private static void ValidateQueryConfig(QueryConfig queryConfig)
        //{
        //    ArgumentNullException.ThrowIfNull(queryConfig, nameof(QueryConfig.ProviderName));
        //}

        //private static void AssignCommandIfAvailable(TableOperationCommand tableOperationCommand, QueryCommand queryCommand)
        //{
        //    if (tableOperationCommand == null) return;

        //    queryCommand.Text = tableOperationCommand.Text;
        //    queryCommand.DefaultValues = tableOperationCommand.DefaultValues;
        //}

        //public static void ExecuteQueryAction(GridAction gridAction, Dictionary<string, IDataProvider> dataProviders)
        //{

        //	gridAction.Perform(dataProviders);
        //}

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
