using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Common.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
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
		private readonly string _ownControlName;

		public SaveGridAction(string ownControlName)
		{
			_ownControlName = ownControlName;
		}

		public override void Perform(Dictionary<string, Sprout.Core.Models.DataAdapters.IDataAdapter> dataAdapters)
		{
			if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
			{
				//find a nice way to route logs to the screen

				throw new NotImplementedException();
			}

			// Implementation for saving the query changes to the database
			foreach (System.Data.DataRow dataRow in ownDataAdapter.DataProvider.Data.Rows)
			{
#warning add try catch block here to not crash the whole app if something goes wrong
				switch (dataRow.RowState)
				{
					case DataRowState.Detached:
						break;
                    case DataRowState.Unchanged:
						break;
					case DataRowState.Added:
						AddNew(ownDataAdapter.InsertCommand, dataRow);
						break;
					case DataRowState.Deleted:
						break;
					case DataRowState.Modified:
						Modify(ownDataAdapter.UpdateCommand, dataRow);
						break;
					default:
						break;
				}
			}

			new DataProviderService().ProvideData(ownDataAdapter.DataProvider);
		}

		private static void AddNew(IEditCommand editCommand, DataRow dataRow)
		{
            if (editCommand is not SqlServerEditCommand sqlEditCommand)
                throw new NotImplementedException();

            var command = sqlEditCommand.Text;

			if (string.IsNullOrWhiteSpace(command))
				throw new Exception("Insert command not set");

			var requestedParameters = ParameterParser.ParseQueryParameters(sqlEditCommand.Text);

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

			using (var conn = new SqlConnection(sqlEditCommand.ConnectionString))
			using (var cmd = new SqlCommand(command, conn))
			{
				AttachParameters(cmd, sqlParams);
				conn.Open();

				var insertedId = cmd.ExecuteScalar();

				conn.Close();
			}
		}

		private void Modify(IEditCommand editCommand, System.Data.DataRow dataRow)
		{
			if (editCommand is not SqlServerEditCommand sqlEditCommand)
				throw new NotImplementedException();

			var command = sqlEditCommand.Text;

			if (string.IsNullOrWhiteSpace(command))
				throw new Exception("Update command not set");

			var requestedParameters = ParameterParser.ParseQueryParameters(sqlEditCommand.Text);

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
			using (var conn = new SqlConnection(sqlEditCommand.ConnectionString))
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
				if ((p.Direction == System.Data.ParameterDirection.InputOutput) && (p.Value == null))
				{
					p.Value = DBNull.Value;
				}

				command.Parameters.Add(p);
			}
		}

		private static void SetQueryParam(QueryParameter queryParam, System.Data.DataRow dataRow)
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
