using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Queries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sprout.Core.Services.Queries.QueryService;

namespace Sprout.Core.Models.GridActions
{
    public class SaveGridAction : GridAction
    {
        private readonly string _gridQueryName;

        public SaveGridAction(string gridQueryName)
        {
            _gridQueryName = gridQueryName;
        }

        public override void Perform(Dictionary<string, Query> queries)
        {
            if (!queries.TryGetValue(_gridQueryName, out var ownQuery))
            {
                //find a nice way to route logs to the screen

                throw new NotImplementedException();
            }

            // Implementation for saving the query changes to the database
            foreach (DataRow dataRow in ownQuery.Data.Rows)
            {
#warning add try catch block here to not crash the whole app if something goes wrong
                switch (dataRow.RowState)
                {
                    case DataRowState.Detached:
                        break;
                    case DataRowState.Unchanged:
                        break;
                    case DataRowState.Added:
                        AddNew(ownQuery, dataRow);
                        break;
                    case DataRowState.Deleted:
                        break;
                    case DataRowState.Modified:
                        Modify(ownQuery, dataRow);
                        break;
                    default:
                        break;
                }
            }

            QueryService.ExecuteQuery(ownQuery);
        }

        private static void AddNew(Query ownQuery, DataRow dataRow)
        {
            var command = ownQuery.InsertCommand.Text;
            var requestedParameters = ParameterParser.ParseQueryParameters(ownQuery.InsertCommand.Text);

            List<SqlParameter> sqlParams = [];

            foreach (var queryParam in requestedParameters)
            {
                SetQueryParam(queryParam, dataRow);

                var param = new SqlParameter
                {
                    ParameterName = $"@{queryParam.Name}",
                    Value = queryParam.Value ?? DBNull.Value
                };

                sqlParams.Add(param);

                command = command.Replace($"{{{queryParam.RawPatameter}}}", $"{param.ParameterName}");

            }

#warning how will this work when called after stored procedure?
#warning maybe have different command types for stored procedure vs text
            command += "; SELECT SCOPE_IDENTITY();";

            using (var conn = new SqlConnection(ownQuery.ConnectionString))
            using (var cmd = new SqlCommand(command, conn))
            {
                AttachParameters(cmd, sqlParams);
                conn.Open();

                var insertedId = cmd.ExecuteScalar();

                //dataRow["TODO: Key"] = insertedId;

                //dataRow.SetAdded();

                conn.Close();
            }
        }

        private void Modify(Query ownQuery, DataRow dataRow)
        {
            var command = ownQuery.UpdateCommand.Text;
            var requestedParameters = ParameterParser.ParseQueryParameters(ownQuery.UpdateCommand.Text);

            List<SqlParameter> sqlParams = [];

            foreach (var queryParam in requestedParameters)
            {
                SetQueryParam(queryParam, dataRow);

                var param = new SqlParameter
                {
                    ParameterName = $"@{queryParam.Name}",
                    Value = queryParam.Value ?? DBNull.Value
                };

                sqlParams.Add(param);

                command = command.Replace($"{{{queryParam.RawPatameter}}}", $"{param.ParameterName}", StringComparison.CurrentCultureIgnoreCase);
            }

#warning maybe have different command types for stored procedure vs text

            #warning better to use a single connection for all operations
            using (var conn = new SqlConnection(ownQuery.ConnectionString))
            using (var cmd = new SqlCommand(command, conn))
            {
                AttachParameters(cmd, sqlParams);
                conn.Open();

                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        private static void AttachParameters(SqlCommand command, IEnumerable<SqlParameter> commandParameters)
        {
            foreach (SqlParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }



        private static void BindParameters(IEnumerable<QueryParameter> parameters, QueryCommand queryCmd, Dictionary<string, Query> pageQueries)
        {
            foreach (var parameter in parameters)
            {
                //no path, take from own
                if (string.IsNullOrEmpty(parameter.Path))
                {

                }
            }
        }

        private static void SetQueryParam(QueryParameter queryParam, DataRow dataRow)
        {
            var value = dataRow[queryParam.Name];

            if (value == DBNull.Value)
            {
                //check for defaul value
            }
            else
            {
                queryParam.Value = value;
            }
        }
    }
}
