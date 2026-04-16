using Microsoft.Data.SqlClient;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.UIStates;
using System.Data;
using System.Windows;
using System.Windows.Data;
#nullable disable

namespace Sprout.Core.Services.SqlServer
{
    public class SqlServerDataService : IDataService
    {
        private SqlConnection _connection;
        private SqlServerDataAdapter _dataAdapter;
        private SqlServerDataProvider _dataProvider;

        public UiStateRegistry UiStateRegistry { get; }

        public SqlServerDataService(SqlServerDataAdapter dataAdapter, UiStateRegistry uiStateRegistry)
        {
            _connection = new SqlConnection(dataAdapter.ConnectionString);
            _dataAdapter = dataAdapter;
            _dataProvider = dataAdapter.DataProvider as SqlServerDataProvider;
            UiStateRegistry = uiStateRegistry;
        }

        public async Task<IEnumerable<ActionMessage>> Insert(DataRow dataRow)
        {
            if (_dataAdapter.InsertCommand is not SqlServerEditCommand sqlEditCommand)
                throw new NotImplementedException();

            var command = sqlEditCommand.Text;

            if (string.IsNullOrWhiteSpace(command))
                throw new Exception($"{nameof(_dataAdapter.InsertCommand)} not set");

            return await Change(sqlEditCommand, dataRow);
        }

        public async Task<IEnumerable<ActionMessage>> Update(DataRow dataRow)
        {
            if (_dataAdapter.UpdateCommand is not SqlServerEditCommand sqlEditCommand)
                throw new NotImplementedException();

            var command = sqlEditCommand.Text;

            if (string.IsNullOrWhiteSpace(command))
                throw new Exception($"{nameof(_dataAdapter.UpdateCommand)} not set");

            return await Change(sqlEditCommand, dataRow);
        }

        public async Task<IEnumerable<ActionMessage>> Delete(DataRow dataRow)
        {
            if (_dataAdapter.DeleteCommand is not SqlServerEditCommand sqlEditCommand)
                throw new NotImplementedException();

            var command = sqlEditCommand.Text;

            if (string.IsNullOrWhiteSpace(command))
                throw new Exception($"{nameof(_dataAdapter.DeleteCommand)} not set");

            return await Change(sqlEditCommand, dataRow);
        }

        public object ResolveBindingPath(object source, string path)
        {
            if (source == null) return null;

            // 1. Create the dummy
            var dummy = new FrameworkElement { DataContext = source };
            var binding = new Binding(path) { Source = source };

            try
            {
                // 2. Attach the binding
                BindingOperations.SetBinding(dummy, FrameworkElement.TagProperty, binding);

                // 3. Capture the value
                return dummy.Tag;
            }
            finally
            {
                // 4. CLEAN UP: Explicitly break the link between the source and the dummy
                BindingOperations.ClearBinding(dummy, FrameworkElement.TagProperty);
                dummy.DataContext = null;
            }
        }

        private async Task<IEnumerable<ActionMessage>> Change(SqlServerEditCommand editCmd, DataRow dataRow)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            var commandText = editCmd.Text;

            var requestedParameters = ParameterParser.ParseQueryParameters(editCmd.Text);

            List<SqlParameter> sqlParams = [];

            foreach (var queryParam in requestedParameters)
            {
                if (queryParam.IsFromUIState)
                {
                    var dep = ParameterParser.ParseDependency(queryParam.RawPatameter);

                    var uiState = UiStateRegistry[dep.ControlName];

                    if (uiState == null)
                    {
                        throw new Exception($"UI State with path {queryParam.Path} not found for parameter {queryParam.Name}");
                    }

                    queryParam.Value = ResolveBindingPath(uiState, dep.PropertyPath);
                }
                else
                {
                    SetQueryParamFromDataRow(queryParam, dataRow);
                }

                var param = new SqlParameter
                {
                    //Replace is needed when the parameter is from the UIRegistry, then it contains multiple .
                    ParameterName = $"@{queryParam.Name.Replace(".", "_")}",
                    Value = queryParam.Value ?? DBNull.Value
                };

                if (!sqlParams.Any(p => string.Equals(p.ParameterName, param.ParameterName, StringComparison.OrdinalIgnoreCase)))
                {
                    sqlParams.Add(param);
                }

                commandText = commandText.Replace($"{{{queryParam.RawPatameter}}}", $"{param.ParameterName}", StringComparison.OrdinalIgnoreCase);
            }

            if (editCmd.WithMessages)
            {
                commandText = CreateMessagesTable(commandText);
            }

            using (var cmd = new SqlCommand(commandText, _connection))
            {
                AttachParameters(cmd, sqlParams);

                if (editCmd.WithMessages)
                {
                    var messages = new List<ActionMessage>();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        //do
                        //{
                            //here i could identify if the columns of the reader match (reader.GetName())
                            //the class signature to match them in a more robust way
                            while (await reader.ReadAsync())
                            {
                                messages.Add(new(reader.GetString(0), reader.GetString(1)));
                            }
                        //} while (await reader.NextResultAsync());
                    }

                    return messages;
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                    return [];
                }
            }
        }



        private string CreateMessagesTable(string commandText)
        {
            commandText =
                $"""
                CREATE TABLE #Messages (Type VARCHAR(15), Message NVARCHAR(MAX));

                {commandText}

                SELECT * FROM #Messages
                """;
            return commandText;
        }

        public async Task ProvideData()
        {
#warning separate query building from query execution
            var queryText = _dataProvider.Text;

            if (string.IsNullOrEmpty(queryText))
            {
                return;
            }

            var dependencyParameters = new List<SqlParameter>();
            var idx = 0;

            foreach (var item in _dataProvider.Dependencies)
            {
                var paramName = $"@depParam{idx}";
                queryText = queryText.Replace($"{{{item.RawDependency}}}", paramName);
                SqlParameter sqlParameter = new SqlParameter(paramName, item.Value ?? DBNull.Value);
                dependencyParameters.Add(sqlParameter);
                idx++;
            }

            var filterStatements = new List<string>();

            if (_dataProvider.Filters.Count > 0)
            {
                int filterIdx = 0;

                foreach (var filter in _dataProvider.Filters.Values)
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

            using (var cmd = new SqlCommand(queryText, _connection))
            {
                cmd.Parameters.AddRange(dependencyParameters.ToArray());

                //prevents the UI from freezing if the connection is slow

                if (_connection.State == ConnectionState.Closed)
                {
                    await _connection.OpenAsync();
                }

                var dt = DataTableFactory.Create();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // Move the CPU-heavy loading to a background thread
                    // This prevents the UI from freezing during the data parsing
                    await Task.Run(() => dt.Load(reader));

                    DataTableFactory.PostLoadLogic(dt);

                    _dataProvider.Data = dt;
                }
            }
        }

        private static void SetQueryParamFromDataRow(QueryParameter queryParam, System.Data.DataRow dataRow)
        {
            // Check if the row is deleted first to avoid the exception
            var version = dataRow.RowState == DataRowState.Deleted
                          ? DataRowVersion.Original
                          : DataRowVersion.Current;

            var value = dataRow[queryParam.Name, version];

            if (value == DBNull.Value)
            {
                // check for default value
            }
            else
            {
                queryParam.Value = value;
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

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public class QueryParameter
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public object Value { get; set; }
            public bool IsMandatory { get; set; }
            public string RawPatameter { get; internal set; }
            public bool IsFromUIState { get; internal set; }
        }
    }
}
