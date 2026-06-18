using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using System.Diagnostics;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.Logging;
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
        private readonly ISqlQueryLogger _sqlQueryLogger;

        public UiStateRegistry UiStateRegistry { get; }

        public SqlServerDataService(SqlServerDataAdapter dataAdapter, UiStateRegistry uiStateRegistry, ISqlQueryLogger sqlQueryLogger = null)
        {
            _connection = SqlServerConnectionFactory.Create(dataAdapter.ConnectionString);
            _dataAdapter = dataAdapter;
            _dataProvider = dataAdapter.DataProvider as SqlServerDataProvider;
            UiStateRegistry = uiStateRegistry;
            _sqlQueryLogger = sqlQueryLogger;
        }

        public async Task<ChangeResult> Insert(DataRow dataRow)
        {
            if (_dataAdapter.InsertCommand is not SqlServerEditCommand sqlEditCommand)
                throw new NotImplementedException();

            var command = sqlEditCommand.Text;

            if (string.IsNullOrWhiteSpace(command))
                throw new Exception($"{nameof(_dataAdapter.InsertCommand)} not set");

            return await Change(sqlEditCommand, dataRow);
        }

        public async Task<ChangeResult> Update(DataRow dataRow)
        {
            if (_dataAdapter.UpdateCommand is not SqlServerEditCommand sqlEditCommand)
                throw new NotImplementedException();

            var command = sqlEditCommand.Text;

            if (string.IsNullOrWhiteSpace(command))
                throw new Exception($"{nameof(_dataAdapter.UpdateCommand)} not set");

            return await Change(sqlEditCommand, dataRow);
        }

        public async Task<ChangeResult> Delete(DataRow dataRow)
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

        public async Task<ChangeResult> Change(IEditCommand editCmd, DataRow dataRow)
        {
            SetBusy(true);

            try
            {
                return await ChangeInternal(editCmd, dataRow);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task<ChangeResult> ChangeInternal(IEditCommand editCmd, DataRow dataRow)
        {
            if (editCmd is not SqlServerEditCommand editCommand)
                throw new NotImplementedException();

            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            var commandText = editCommand.Text;

            var requestedParameters = ParameterParser.ParseQueryParameters(editCommand.Text);

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

            if (editCommand.WithMessages)
            {
                commandText = CreateMessagesTable(commandText);
            }

            var changeResult = new ChangeResult();

            using (var cmd = new SqlCommand(commandText, _connection))
            {
                AttachParameters(cmd, sqlParams);

                var sw = Stopwatch.StartNew();
                var isNextResultFetched = false;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    sw.Stop();
                    _sqlQueryLogger?.Log(nameof(SqlServerDataService), cmd.CommandText, cmd.Parameters, sw.Elapsed);
                    do
                    {
                        //This helped when you have an early return in a change command
                        if (reader.IsClosed) break;

                        if (isNextResultFetched)
                            isNextResultFetched = false;

                        if (IsMessages(reader))
                        {
                            while (await reader.ReadAsync())
                            {
                                changeResult.Messages.Add(new(reader.GetStringOrNull(0), reader.GetStringOrNull(1)));
                            }
                        }
                        else if (IsResult(reader))
                        {
                            if (await reader.ReadAsync())
                            {
                                var resultType = reader.GetFieldType(0);

                                if (resultType == typeof(int))
                                {
                                    changeResult.Result = (bool)Convert.ChangeType(reader.GetIntOrNull(0), typeof(bool));
                                }
                                else if (resultType == typeof(bool))
                                {
                                    changeResult.Result = reader.GetBoolOrNull(0);
                                }
                                else
                                {
                                    throw new Exception($"Unsupported result type {resultType}.");
                                }
                            }
                        }
                        else
                        {
                            if (changeResult.ExtraData != null)
                                throw new Exception("You can return only one ExtraData result-set.");

                            var dt = new DataTable();
                            await Task.Run(() => dt.Load(reader)); //This calls reader.NextResult()
                            changeResult.ExtraData = dt;
                            isNextResultFetched = true;
                        }

                    } while (isNextResultFetched || await reader.NextResultAsync());
                }

                return changeResult;
            }
        }

        private static bool IsMessages(SqlDataReader reader)
        {
            //ActionMessage has 2 properties
            if (reader.FieldCount != 2)
            {
                return false;
            }

            if (reader.GetName(0) != "Type")
            {
                return false;
            }

            if (reader.GetName(1) != "Message")
            {
                return false;
            }

            return true;
        }

        private static bool IsResult(SqlDataReader reader)
        {
            //Result has 1 property
            if (reader.FieldCount != 1)
            {
                return false;
            }

            if (reader.GetName(0) != "Result")
            {
                return false;
            }

            return true;
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
            SetBusy(true);

            try
            {
                (var queryText, var dependencyParameters) = BuildQueryAndParameters();

                //a button which used to have a select action but now has only update action
                if (string.IsNullOrEmpty(queryText)) return;

                using var cmd = new SqlCommand(queryText, _connection);
                cmd.Parameters.AddRange(dependencyParameters.ToArray());

                //prevents the UI from freezing if the connection is slow
                if (_connection.State == ConnectionState.Closed)
                {
                    await _connection.OpenAsync();
                }

                var dt = DataTableFactory.Create();

                var sw = Stopwatch.StartNew();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    sw.Stop();
                    _sqlQueryLogger?.Log(nameof(SqlServerDataService), cmd.CommandText, cmd.Parameters, sw.Elapsed);

                    if (reader.FieldCount == 0 && _dataAdapter.ParentType == typeof(SproutDataGridConfig))
                    {
                        throw new Exception($"Critical Error: Query of grid {_dataAdapter.Name} is not returning any columns!");
                    }

                    reader.LoadDataTableColumnsFromSchema(dt);

                    // Move the CPU-heavy loading to a background thread
                    // This prevents the UI from freezing during the data parsing
                    await Task.Run(() => dt.Load(reader));

                    DataTableFactory.PostLoadLogic(dt);

                    _dataProvider.Data = dt;
                }
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool isBusy)
        {
            if (UiStateRegistry.Get(_dataAdapter.Name) is not BusyUIState busyState)
                return;

            busyState.IsBusy = isBusy;
        }

        private (string queryText, List<SqlParameter> dependencyParameters) BuildQueryAndParameters()
        {
            var result = QueryBuilder.Build(_dataProvider.Text, _dataProvider);

            var sqlParams = result.Parameters
                .Select(p => new SqlParameter(p.Name, p.Value))
                .ToList();

            return (result.QueryText, sqlParams);
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
