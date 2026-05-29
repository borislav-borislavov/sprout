using DuckDB.NET.Data;
using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Services.Duck
{
    public class DuckDbDataService : IDataService
    {
        private DuckDBConnection _connection;
        private DuckDataAdapter _duckDataAdapter;
        private DuckDataProvider _dataProvider;

        public UiStateRegistry UiStateRegistry { get; }

        public DuckDbDataService(DuckDataAdapter duckDataAdapter, UiStateRegistry uiStateRegistry)
        {
            _duckDataAdapter = duckDataAdapter;
            _dataProvider = duckDataAdapter.DataProvider as DuckDataProvider;
            UiStateRegistry = uiStateRegistry;
            _connection = new DuckDBConnection(duckDataAdapter.ConnectionString);
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
            var requestedParameters = ParameterParser.ParseQueryParameters(editCommand.Text);

            var duckParams = new List<(string Name, object Value)>();

            foreach (var queryParam in requestedParameters)
            {
                if (queryParam.IsFromUIState)
                {
                    var dep = ParameterParser.ParseDependency(queryParam.RawPatameter);
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
                p.ParameterName = name;
                p.Value = value;
                cmd.Parameters.Add(p);
            }

            var isNextResultFetched = false;

            using var reader = await cmd.ExecuteReaderAsync();

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
            var queryText = _dataProvider.Text;

            if (string.IsNullOrWhiteSpace(_dataProvider.Text))
                throw new Exception("No query provided!");

            var parameters = new List<(string Name, object Value)>();
            var idx = 0;

            foreach (var item in _dataProvider.Dependencies)
            {
                var paramName = $"$depParam{idx}";
                queryText = queryText.Replace($"{{{item.RawDependency}}}", paramName);
                parameters.Add((paramName, item.Value ?? DBNull.Value));
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
                        filterIdx++;
                        continue;
                    }

                    string filterStatement;

                    if (filter.IsRange)
                    {
                        var startParamName = $"$filter_{filterIdx}_Start{idx}";
                        parameters.Add((startParamName, filter.StartValue ?? DBNull.Value));
                        idx++;

                        var endParamName = $"$filter_{filterIdx}_End{idx}";
                        parameters.Add((endParamName, filter.EndValue ?? DBNull.Value));
                        idx++;

                        filterStatement = string.Format(filter.Text, startParamName, endParamName);
                    }
                    else
                    {
                        var paramName = $"$filter_{filterIdx}_{idx}";
                        parameters.Add((paramName, filter.StartValue ?? DBNull.Value));
                        idx++;

                        filterStatement = string.Format(filter.Text, paramName);
                    }

                    filterStatements.Add(filterStatement);
                    filterIdx++;
                }
            }

            if (filterStatements.Count > 0
                && queryText.IndexOf(Const.SqlServer.WhereFilter, StringComparison.OrdinalIgnoreCase) == -1
                && queryText.IndexOf(Const.SqlServer.AndFilter, StringComparison.OrdinalIgnoreCase) == -1)
            {
                throw new Exception(
                    $"Filters are added but {Const.SqlServer.WhereFilter} or {Const.SqlServer.AndFilter} not used in query");
            }

            if (queryText.IndexOf(Const.SqlServer.WhereFilter, StringComparison.OrdinalIgnoreCase) != -1)
            {
                var replacement = filterStatements.Count == 0
                    ? string.Empty
                    : $"WHERE {string.Join($"{Environment.NewLine}AND ", filterStatements)}";

                queryText = queryText.Replace(
                    Const.SqlServer.WhereFilter, replacement, StringComparison.OrdinalIgnoreCase);
            }

            if (queryText.IndexOf(Const.SqlServer.AndFilter, StringComparison.OrdinalIgnoreCase) != -1)
            {
                var replacement = filterStatements.Count == 0
                    ? string.Empty
                    : $" AND {string.Join($"{Environment.NewLine}AND ", filterStatements)}";

                queryText = queryText.Replace(
                    Const.SqlServer.AndFilter, replacement, StringComparison.OrdinalIgnoreCase);
            }

            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = queryText;

            foreach (var (name, value) in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value;
                cmd.Parameters.Add(p);
            }

            var dt = DataTableFactory.Create();

            using var reader = await cmd.ExecuteReaderAsync();

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
            SqlServerDataService.QueryParameter queryParam, DataRow dataRow)
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