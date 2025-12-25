using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.Queries;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.UIStates;
using Sprout.Core.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Services.Queries
{
    public class QueryService
    {

        private static List<SqlParameter> AddDependencyParameters(Query query)
        {
            var dependencyParameters = new List<SqlParameter>();

            //SqlParameter
            foreach (var item in query.Dependencies)
            {
                SqlParameter sqlParameter = new SqlParameter(item.RawDependency, item.Value);
                dependencyParameters.Add(sqlParameter);
            }

            return dependencyParameters;
        }

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

            //i could have a syntax {@UserStateID ? 1,}
            //which means take UserStateID if not null or else take 1
            //@UserStateID ! menans take UserStateID and if null throw an error

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

            //if (string.IsNullOrWhiteSpace(queryCmd.Text))
            //{
            //    //TODO: infer the command if text is empty
            //}
            //else
            //{
            //    var requestedQueryParams = ParseParameters(queryCmd);


            //    BindParameters(requestedQueryParams, queryCmd, _pageQueries);

            //    switch (queryCmd.Type)
            //    {
            //        //case QueryCommandTypes.AddRow:
            //        //    ExecuteAddRowCommand(queryCmd, requestedQueryParams);
            //        //    break;
            //        case QueryCommandTypes.Insert:
            //            break;
            //        case QueryCommandTypes.Update:
            //            break;
            //        case QueryCommandTypes.Delete:
            //            break;
            //        //case QueryCommandTypes.Save:
            //        //    ExecuteSaveCommand(queryCmd, requestedQueryParams);
            //            break;
            //        case QueryCommandTypes.Execute:
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }

        //private static void ExecuteSaveCommand(QueryCommand queryCmd, IEnumerable<QueryParameter> requestedQueryParams)
        //{
        //    foreach (DataRow dataRow in queryCmd.Parent.Data.Rows)
        //    {
        //        if (dataRow.RowState == DataRowState.Added)
        //        {
        //            var query = queryCmd.Parent.InsertCommand.Text;

        //            foreach (var queryParam in requestedQueryParams)
        //            {
        //                SetQueryParam(queryParam, dataRow);

        //                query = query.Replace($"{{{queryParam.RawPatameter}}}", $"{queryParam.Value}");
        //            }


        //        }
        //    }
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
