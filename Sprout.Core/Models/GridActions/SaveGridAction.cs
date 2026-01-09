using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Common.Models;
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
		private readonly string _dataProviderName;

		public SaveGridAction(string dataProviderName)
		{
			_dataProviderName = dataProviderName;
		}

		public override void Perform(Dictionary<string, IDataProvider> dataProviders)
		{
			if (!dataProviders.TryGetValue(_dataProviderName, out var dataProvider))
			{
				//find a nice way to route logs to the screen

				throw new NotImplementedException();
			}

			// Implementation for saving the query changes to the database
			foreach (DataRow dataRow in dataProvider.Data.Rows)
			{
#warning add try catch block here to not crash the whole app if something goes wrong
				switch (dataRow.RowState)
				{
					case DataRowState.Detached:
						break;
					case DataRowState.Unchanged:
						break;
					case DataRowState.Added:
						AddNew(dataProvider, dataRow);
						break;
					case DataRowState.Deleted:
						break;
					case DataRowState.Modified:
						Modify(dataProvider, dataRow);
						break;
					default:
						break;
				}
			}

			new DataProviderService().ProvideData(dataProvider);
			//QueryService.ExecuteQuery(dataProvider);
		}

		private static void AddNew(IDataProvider dataProvider, DataRow dataRow)
		{
			if (dataProvider is not Query query)
				throw new Exception();

			var command = query.InsertCommand.Text;

			if (string.IsNullOrWhiteSpace(command))
				throw new Exception("Insert command not set");

			var requestedParameters = ParameterParser.ParseQueryParameters(query.InsertCommand.Text);

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

			using (var conn = new SqlConnection(query.ConnectionString))
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

		private void Modify(IDataProvider dataProvider, DataRow dataRow)
		{
			if (dataProvider is not Query query)
				throw new Exception();

			var command = query.UpdateCommand.Text;

			if (string.IsNullOrWhiteSpace(command))
				throw new Exception("Update command not set");

			var requestedParameters = ParameterParser.ParseQueryParameters(query.UpdateCommand.Text);

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
			using (var conn = new SqlConnection(query.ConnectionString))
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
