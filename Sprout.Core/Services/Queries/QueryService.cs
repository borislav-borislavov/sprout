using Microsoft.Data.SqlClient;
using Sprout.Core.Models.Configurations.Queries;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.UIStates;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Services.Queries
{
    public class QueryService
    {
        public static void ExecuteQuery(Query query)
        {
            var queryText = query.Text;

            var dependencyParameters = new List<SqlParameter>();
            var idx = 0;

            foreach (var item in query.Dependencies)
            {
                var paramName = $"@depParam{idx}";
                queryText = queryText.Replace($"{{{item.RawDependency}}}", paramName);
                SqlParameter sqlParameter = new SqlParameter(paramName, item.Value ?? DBNull.Value);
                dependencyParameters.Add(sqlParameter);
                idx++;
            }

            using (var conn = new SqlConnection(query.ConnectionString))
            using (var cmd = new SqlCommand(queryText, conn))
            {
                cmd.Parameters.AddRange(dependencyParameters.ToArray());

                using (var da = new SqlDataAdapter(cmd))
                {
                    //cmd.Parameters.AddRange(AddDependencyParameters(query).ToArray());
                    //if (parameters != null && parameters.Length > 0)
                    //    cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    query.Data.Clear();
                    da.Fill(query.Data);
                }
            }

        }

#warning move this to a factory for consistency with the design
        public static Query CreateQuery(QueryConfig queryConfig)
        {
            ValidateQueryConfig(queryConfig);

            var query = new Query
            {
                Name = queryConfig.Name,
                Text = queryConfig.Text,
                TableName = queryConfig.TableName,
                ConnectionString = queryConfig.ConnectionString
            };

            query.Dependencies = ParameterParser.ParseDependencies(query.Text);

            AssignCommandIfAvailable(queryConfig.InsertCommand, query.InsertCommand);
            AssignCommandIfAvailable(queryConfig.UpdateCommand, query.UpdateCommand);
            AssignCommandIfAvailable(queryConfig.DeleteCommand, query.DeleteCommand);

            return query;
        }

        public static void BindDependencies(Query query, UiStateRegistry uiStateRegistry)
        {
            foreach (var dep in query.Dependencies)
            {
                BindingOperations.SetBinding(
                    target: dep,
                    QueryDependency.ValueProperty,
                    new Binding
                    {
                        Source = uiStateRegistry,
                        Path = new PropertyPath($"[{dep.ControlName}].{dep.PropertyPath}")
                    });
            }
        }

        private static void ValidateQueryConfig(QueryConfig queryConfig)
        {
            ArgumentNullException.ThrowIfNull(queryConfig, nameof(QueryConfig.Name));
        }

        private static void AssignCommandIfAvailable(TableOperationCommand tableOperationCommand, QueryCommand queryCommand)
        {
            if (tableOperationCommand == null) return;

            queryCommand.Text = tableOperationCommand.Text;
            queryCommand.DefaultValues = tableOperationCommand.DefaultValues;
        }

        public static void ExecuteQueryAction(GridAction gridAction, Dictionary<string, Query> _pageQueries)
        {

            gridAction.Perform(_pageQueries);
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
