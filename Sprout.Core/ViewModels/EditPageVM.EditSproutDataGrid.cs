using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DuckDB.NET.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.Api;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Configurations.Duck;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Dialog;
using System.Data;
using System.Data.Common;
using System.Net.Http;
#nullable disable

namespace Sprout.Core.ViewModels
{
    public partial class EditPageVM : ObservableObject
    {
        [ObservableProperty]
        private SproutDataGridConfig _selectedDataGrid;

        [ObservableProperty]
        private SproutDataGridColumnConfig _selectedColumn;

        [ObservableProperty]
        private bool _isComboColumnSelected;

        [ObservableProperty]
        private ObservableObject _selectedColumnAdapterViewModel;

        [ObservableProperty]
        private string _selectedComboColAdapterType;

        partial void OnSelectedColumnChanged(SproutDataGridColumnConfig value)
        {
            UpdateComboColumnState(value);
        }

        private void UpdateComboColumnState(SproutDataGridColumnConfig column)
        {
            if (column != null && column.ColumnType == ColumnType.Combo)
            {
                IsComboColumnSelected = true;

                //if (column.DataAdapter is SqlServerDataAdapterConfig sqlConfig)
                //{
                //    SelectedColumnAdapterViewModel = new SqlServerReadOnlyDataAdapterVM(sqlConfig);
                //}
                //else
                //{
                //    SelectedColumnAdapterViewModel = null;
                //}
            }
            else
            {
                IsComboColumnSelected = false;
                SelectedColumnAdapterViewModel = null;
            }
        }

        [RelayCommand]
        private void InitializeComboAdapter()
        {
            if (SelectedColumn == null || SelectedColumn.ColumnType != ColumnType.Combo) return;

            _navigationService.ShowManageAdapter(SelectedColumn);
        }

        [RelayCommand]
        private void AddColumn()
        {
            if (SelectedDataGrid == null) return;

            SelectedDataGrid.Columns.Add(new SproutDataGridColumnConfig());
        }

        [RelayCommand]
        private void DeleteColumn()
        {
            if (SelectedColumn != null)
            {
                SelectedDataGrid.Columns.Remove(SelectedColumn);
            }
        }

        [RelayCommand]
        private void MoveColumnUp()
        {
            if (SelectedDataGrid == null || SelectedColumn == null) return;

            var index = SelectedDataGrid.Columns.IndexOf(SelectedColumn);
            if (index <= 0) return;

            SelectedDataGrid.Columns.Move(index, index - 1);
        }

        [RelayCommand]
        private void MoveColumnDown()
        {
            if (SelectedDataGrid == null || SelectedColumn == null) return;

            var index = SelectedDataGrid.Columns.IndexOf(SelectedColumn);
            if (index < 0 || index >= SelectedDataGrid.Columns.Count - 1) return;

            SelectedDataGrid.Columns.Move(index, index + 1);
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

        [RelayCommand]
        private async Task CompleteColumns()
        {
            if (SelectedDataGrid == null)
            {
                return;
            }
#warning this should be put in a factory which repsonds properly based on the type of adapter
            if (SelectedDataGrid.DataAdapter is SqlServerDataAdapterConfig adapterConfig)
            {
                SqlServerPopulateColumns(adapterConfig);
                return;
            }
            else if (SelectedDataGrid.DataAdapter is DuckDataAdapterConfig duckAdapterConfig)
            {
                DuckDbPopulateColumns(duckAdapterConfig);
            }
            else if (SelectedDataGrid.DataAdapter is ApiDataAdapterConfig apiAdapterConfig)
            {
                await ApiPopulateColumns(apiAdapterConfig);
            }
        }

        private static readonly char[] WpfBindingReservedChars = ['.', '/', '[', ']', '(', ')'];

        private static bool ContainsWpfBindingReservedChars(string name)
        {
            return name.IndexOfAny(WpfBindingReservedChars) >= 0;
        }

        private void SqlServerPopulateColumns(SqlServerDataAdapterConfig adapterConfig)
        {
            if (adapterConfig.DataProvider is not SqlServerDataProviderConfig dataProviderConfig) return;

            var query = dataProviderConfig.Text;

            if (string.IsNullOrWhiteSpace(query)) return;

            query = query.Replace("{!whereFilter}", string.Empty, StringComparison.OrdinalIgnoreCase);
            query = query.Replace("{!andFilter}", string.Empty, StringComparison.OrdinalIgnoreCase);

            var requestedParameters = DependencyParser.ParseDependencyMetas(query);
            List<SqlParameter> sqlParams = [];

            foreach (var queryParam in requestedParameters)
            {
                if (string.IsNullOrEmpty(queryParam.Name))
                    continue;

                var safeParamName = $"@{queryParam.Name.Replace(".", "_")}";

                //if a variable is used multiple times in the query we only want to add it once to the parameters collection
                if (sqlParams.Any(sp => sp.ParameterName == safeParamName))
                {
                    continue;
                }

                var param = new SqlParameter
                {
                    ParameterName = safeParamName,
                    Value = DBNull.Value
                };

                sqlParams.Add(param);

                query = query.Replace($"{{{queryParam.RawPatameter}}}", safeParamName, StringComparison.CurrentCultureIgnoreCase);
            }

            var connectionString = adapterConfig.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _configService.Load().Settings.SqlServerConnectionString;
            }

            using (var conn = SqlServerConnectionFactory.Create(connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                try
                {
                    AttachParameters(cmd, sqlParams);

                    conn.Open();

                    using var reader = GetSqlServerReader(cmd);

                    if (reader.CanGetColumnSchema())
                    {
                        var columnSchema = reader.GetColumnSchema();

                        if (columnSchema.Count == 0) throw new Exception("No columns selected!");

                        var invalidColumn = columnSchema.FirstOrDefault(c => ContainsWpfBindingReservedChars(c.ColumnName));
                        if (invalidColumn != null)
                        {
                            SelectedDataGrid.Columns.Clear();
                            _dialogService.ShowMessage(
                                $"Column '{invalidColumn.ColumnName}' contains reserved characters '.', '/', '[', ']', '(', ')'. Use the Header property to provide a display name and alias the column in your query.",
                                "Error", DialogButton.OK, DialogImage.Error);
                            return;
                        }

                        foreach (var column in columnSchema)
                        {
                            var matchingCol = SelectedDataGrid.Columns
                                .FirstOrDefault(c => string.Equals(c.BindingPath, column.ColumnName, StringComparison.OrdinalIgnoreCase));

                            //this is here in order to append a new column easily
                            //but as a side effect if the query has duplicate columns it will ommit them
                            //luckily the issue will be caught when you select the data
                            if (matchingCol == null)
                            {
                                SelectedDataGrid.Columns.Add(new SproutDataGridColumnConfig
                                {
                                    BindingPath = column.ColumnName,
                                    Header = column.ColumnName,
                                    ColumnType = ColumnType.Text,
                                    IsReadOnly = column.IsReadOnly == true || column.IsAutoIncrement == true
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    conn.Close();

                    _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
                }
            }
        }

        private SqlDataReader GetSqlServerReader(SqlCommand cmd)
        {
            try
            {
                return cmd.ExecuteReader(CommandBehavior.SchemaOnly); //this doesn't work when you have a temp table
            }
            catch (Exception)
            {
                return cmd.ExecuteReader(CommandBehavior.SingleRow); //but this does
            }
        }

        private void DuckDbPopulateColumns(DuckDataAdapterConfig adapterConfig)
        {
            if (adapterConfig.DataProvider is not DuckDataProviderConfig dataProviderConfig) return;

            var query = dataProviderConfig.Text;

            if (string.IsNullOrWhiteSpace(query)) return;

            query = query.Replace("{!whereFilter}", string.Empty, StringComparison.OrdinalIgnoreCase);
            query = query.Replace("{!andFilter}", string.Empty, StringComparison.OrdinalIgnoreCase);

            var connectionString = adapterConfig.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _configService.Load().Settings.DuckDbConnectionString;
            }

            //TODO: Missing dependencies replace

            using (var conn = new DuckDBConnection(connectionString))
            using (var cmd = conn.CreateCommand())
            {
                try
                {
                    conn.Open();

                    cmd.CommandText = query;

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        var schemaTable = reader.GetSchemaTable();

                        if (reader.CanGetColumnSchema())
                        {
                            var columnSchema = reader.GetColumnSchema();

                            if (columnSchema.Count == 0) throw new Exception("No columns selected!");

                            var invalidColumn = columnSchema.FirstOrDefault(c => ContainsWpfBindingReservedChars(c.ColumnName));
                            if (invalidColumn != null)
                            {
                                SelectedDataGrid.Columns.Clear();
                                _dialogService.ShowMessage(
                                    $"Column '{invalidColumn.ColumnName}' contains reserved characters '.', '/', '[', ']', '(', ')'. Use the Header property to provide a display name and alias the column in your query.",
                                    "Error", DialogButton.OK, DialogImage.Error);
                                return;
                            }

                            foreach (var column in columnSchema)
                            {
                                var matchingCol = SelectedDataGrid.Columns
                                    .FirstOrDefault(c => string.Equals(c.BindingPath, column.ColumnName, StringComparison.OrdinalIgnoreCase));

                                if (matchingCol == null)
                                {
                                    SelectedDataGrid.Columns.Add(new SproutDataGridColumnConfig
                                    {
                                        BindingPath = column.ColumnName,
                                        Header = column.ColumnName,
                                        ColumnType = ColumnType.Text,
                                        IsReadOnly = column.IsReadOnly == true || column.IsAutoIncrement == true
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    conn.Close();

                    _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
                }
            }
        }

        private async Task ApiPopulateColumns(ApiDataAdapterConfig adapterConfig)
        {
            if (adapterConfig.DataProvider is not ApiDataProviderConfig dataProviderConfig) return;

            var url = dataProviderConfig.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var root = JToken.Parse(json);

                JToken target = root;

                if (!string.IsNullOrWhiteSpace(dataProviderConfig.DataPath))
                {
                    var path = dataProviderConfig.DataPath.StartsWith("$")
                        ? dataProviderConfig.DataPath
                        : "$." + dataProviderConfig.DataPath;
                    target = root.SelectToken(path);

                    if (target == null)
                    {
                        _dialogService.ShowMessage(
                            $"DataPath '{dataProviderConfig.DataPath}' did not match any element in the response.",
                            "Error", DialogButton.OK, DialogImage.Error);
                        return;
                    }
                }

                JArray array = target switch
                {
                    JArray ja => ja,
                    JObject jo => new JArray(jo),
                    _ => null
                };

                if (array == null || array.Count == 0 || array[0] is not JObject firstObj)
                {
                    _dialogService.ShowMessage(
                        "The API response did not return a non-empty array of objects.",
                        "Error", DialogButton.OK, DialogImage.Error);
                    return;
                }

                foreach (var prop in firstObj.Properties())
                {
                    if (ContainsWpfBindingReservedChars(prop.Name))
                    {
                        SelectedDataGrid.Columns.Clear();
                        _dialogService.ShowMessage(
                            $"Column '{prop.Name}' contains reserved characters '.', '/', '[', ']', '(', ')'. Alias the field in your API or use a DataPath.",
                            "Error", DialogButton.OK, DialogImage.Error);
                        return;
                    }

                    var matchingCol = SelectedDataGrid.Columns
                        .FirstOrDefault(c => string.Equals(c.BindingPath, prop.Name, StringComparison.OrdinalIgnoreCase));

                    if (matchingCol == null)
                    {
                        SelectedDataGrid.Columns.Add(new SproutDataGridColumnConfig
                        {
                            BindingPath = prop.Name,
                            Header = prop.Name,
                            ColumnType = ColumnType.Text,
                            IsReadOnly = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }
    }
}
