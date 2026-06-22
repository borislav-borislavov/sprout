using DuckDB.NET.Data;
using Sprout.Core.Common;
using System.Diagnostics;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.Logging;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Data;
using Sprout.Core.Models.Configurations.DataGrid;

namespace Sprout.Core.Services.Duck
{
    public class DuckDbDataService : IDataService
    {
        private DuckDBConnection _connection;
        private DuckDataAdapter _duckDataAdapter;
        private DuckDataProvider _dataProvider;
        private readonly ISqlQueryLogger _sqlQueryLogger;

        public UiStateRegistry UiStateRegistry { get; }

        public DuckDbDataService(DuckDataAdapter duckDataAdapter, UiStateRegistry uiStateRegistry, ISqlQueryLogger sqlQueryLogger = null)
        {
            _duckDataAdapter = duckDataAdapter;
            _dataProvider = duckDataAdapter.DataProvider as DuckDataProvider;
            UiStateRegistry = uiStateRegistry;
            _connection = new DuckDBConnection(duckDataAdapter.ConnectionString);
            _sqlQueryLogger = sqlQueryLogger;
        }

        // ──────────────────────────────────────────────
        // Insert / Update / Delete
        // ──────────────────────────────────────────────

        public async Task<ChangeResult> Insert(DataRow dataRow)
        {
            if (_duckDataAdapter.InsertCommand is not DuckEditCommand editCommand)
                throw new NotImplementedException();

            if (string.IsNullOrWhiteSpace(editCommand.Text))
                throw new Exception($"{nameof(_duckDataAdapter.InsertCommand)} not set");

            return await Change(editCommand, dataRow);
        }

        public async Task<ChangeResult> Update(DataRow dataRow)
        {
            if (_duckDataAdapter.UpdateCommand is not DuckEditCommand editCommand)
                throw new NotImplementedException();

            if (string.IsNullOrWhiteSpace(editCommand.Text))
                throw new Exception($"{nameof(_duckDataAdapter.UpdateCommand)} not set");

            return await Change(editCommand, dataRow);
        }

        public async Task<ChangeResult> Delete(DataRow dataRow)
        {
            if (_duckDataAdapter.DeleteCommand is not DuckEditCommand editCommand)
                throw new NotImplementedException();

            if (string.IsNullOrWhiteSpace(editCommand.Text))
                throw new Exception($"{nameof(_duckDataAdapter.DeleteCommand)} not set");

            return await Change(editCommand, dataRow);
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
            if (editCmd is not DuckEditCommand editCommand)
                throw new NotImplementedException();

            var commandText = editCommand.Text;

            // 1. Parse {param} placeholders from the command text
            var requestedParameters = DependencyParser.ParseDependencyMetas(editCommand.Text);

            var duckParams = new List<(string Name, object Value)>();

            foreach (var queryParam in requestedParameters)
            {
                if (queryParam.IsFromUIState)
                {
                    var dep = DependencyParser.ParseDependency(queryParam.RawPatameter);
                    var uiState = UiStateRegistry[dep.ControlName];

                    if (uiState == null)
                        throw new Exception(
                            $"UI State with path {queryParam.Path} not found for parameter {queryParam.Name}");

                    queryParam.Value = ResolveBindingPath(uiState, dep.PropertyPath);
                }
                else
                {
                    SetQueryParamFromDataRow(queryParam, dataRow);
                }

                // DuckDB uses $name instead of @name.
                // Replace . with _ for UIState-sourced params (e.g. Control.Property → Control_Property).
                var paramName = $"${queryParam.Name.Replace(".", "_")}";

                if (!duckParams.Any(p => string.Equals(p.Name, paramName, StringComparison.OrdinalIgnoreCase)))
                    duckParams.Add((paramName, queryParam.Value ?? DBNull.Value));

                commandText = commandText.Replace(
                    $"{{{queryParam.RawPatameter}}}",
                    paramName,
                    StringComparison.OrdinalIgnoreCase);
            }

            // 2. Optionally wrap with a temp messages table
            //    DuckDB does not support #TempTable syntax; use CREATE TEMPORARY TABLE instead.
            if (editCommand.WithMessages)
                commandText = CreateMessagesTable(commandText);

            var changeResult = new ChangeResult();

            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = commandText;


            foreach (var (name, value) in duckParams)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name.TrimStart('$');
                p.Value = value;
                cmd.Parameters.Add(p);
            }

            var sw = Stopwatch.StartNew();
            var isNextResultFetched = false;

            using var reader = await cmd.ExecuteReaderAsync();
            sw.Stop();
            _sqlQueryLogger?.Log(nameof(DuckDbDataService), cmd.CommandText, cmd.Parameters, sw.Elapsed);

            do
            {
                if (isNextResultFetched)
                    isNextResultFetched = false;

                if (IsMessages(reader))
                {
                    while (await reader.ReadAsync())
                    {
                        changeResult.Messages.Add(
                            new(reader.GetStringOrNull(0), reader.GetStringOrNull(1)));
                    }
                }
                else if (IsResult(reader))
                {
                    if (await reader.ReadAsync())
                    {
                        var resultType = reader.GetFieldType(0);

                        if (resultType == typeof(int))
                        {
                            changeResult.Result = (bool)Convert.ChangeType(reader.GetInt32(0), typeof(bool));
                        }
                        else if (resultType == typeof(bool))
                        {
                            changeResult.Result = reader.GetBoolean(0);
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
                    await Task.Run(() => dt.Load(reader)); // dt.Load calls reader.NextResult() internally
                    changeResult.ExtraData = dt;
                    isNextResultFetched = true;
                }

            } while (isNextResultFetched || await reader.NextResultAsync());

            return changeResult;
        }

        public async Task ProvideData()
        {
            SetBusy(true);

            try
            {
                await ProvideDataInternal();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ProvideDataInternal()
        {
            var result = QueryBuilder.Build(_dataProvider.Text, _dataProvider, "$");

            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = result.QueryText;

            foreach (var param in result.Parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = param.Name.TrimStart('$');
                p.Value = param.Value;
                cmd.Parameters.Add(p);
            }

            var dt = DataTableFactory.Create();

            var sw = Stopwatch.StartNew();
            using var reader = await Task.Run(() => cmd.ExecuteReaderAsync());
            sw.Stop();
            _sqlQueryLogger?.Log(nameof(DuckDbDataService), cmd.CommandText, cmd.Parameters, sw.Elapsed);

            if (reader.FieldCount == 0 && _duckDataAdapter.ParentType == typeof(SproutDataGridConfig))
            {
                throw new Exception($"Critical Error: Query of grid {_duckDataAdapter.Name} is not returning any columns!");
            }

            reader.LoadDataTableColumnsFromSchema(dt);

            await Task.Run(() => dt.Load(reader));

            DataTableFactory.PostLoadLogic(dt);

            _dataProvider.Data = dt;
        }

        private void SetBusy(bool isBusy)
        {
            if (UiStateRegistry.Get(_duckDataAdapter.Name) is not BusyUIState busyState)
                return;

            busyState.IsBusy = isBusy;
        }

        public object ResolveBindingPath(object source, string path)
        {
            if (source == null) return null;

            var dummy = new FrameworkElement { DataContext = source };
            var binding = new Binding(path) { Source = source };

            try
            {
                BindingOperations.SetBinding(dummy, FrameworkElement.TagProperty, binding);
                return dummy.Tag;
            }
            finally
            {
                BindingOperations.ClearBinding(dummy, FrameworkElement.TagProperty);
                dummy.DataContext = null;
            }
        }

        private static bool IsMessages(DbDataReader reader)
        {
            if (reader.FieldCount != 2) return false;
            if (reader.GetName(0) != "Type") return false;
            if (reader.GetName(1) != "Message") return false;
            return true;
        }

        private static bool IsResult(DbDataReader reader)
        {
            if (reader.FieldCount != 1) return false;
            if (reader.GetName(0) != "Result") return false;
            return true;
        }

        private static string CreateMessagesTable(string commandText) =>
            $"""
            CREATE TEMPORARY TABLE _messages (Type VARCHAR, Message VARCHAR);

            {commandText}

            SELECT * FROM _messages;
            """;

        private static void SetQueryParamFromDataRow(
            DependencyMeta queryParam, DataRow dataRow)
        {
            var version = dataRow.RowState == DataRowState.Deleted
                ? DataRowVersion.Original
                : DataRowVersion.Current;

            var value = dataRow[queryParam.Name, version];

            if (value != DBNull.Value)
                queryParam.Value = value;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}