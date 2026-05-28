using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DuckDB.NET.Data;
using Microsoft.Data.SqlClient;
using Sprout.Core.Common;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Configurations.Duck;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            //try
            //{
            //    if (SelectedComboColAdapterType == "SqlServer")
            //    {
            //        SelectedColumn.DataAdapter = new SqlServerDataAdapterConfig
            //        {
            //            //ConnectionString = "Server=.;Database=DbName;Trusted_Connection=True;TrustServerCertificate=Yes",

            //            DataProvider = new SqlServerDataProviderConfig
            //            {
            //                Text = string.Empty
            //            },

            //            InsertCommand = new SqlServerEditCommandConfig(),
            //            UpdateCommand = new SqlServerEditCommandConfig(),
            //            DeleteCommand = new SqlServerEditCommandConfig(),
            //        };
            //    }
            //    else if (SelectedComboColAdapterType == "Duck")
            //    {
            //        SelectedColumn.DataAdapter = new DuckDataAdapterConfig
            //        {
            //            ConnectionString = "DataSource=:memory:",

            //            DataProvider = new DuckDataProviderConfig
            //            {
            //                Text = string.Empty
            //            },

            //            InsertCommand = new DuckEditCommandConfig(),
            //            UpdateCommand = new DuckEditCommandConfig(),
            //            DeleteCommand = new DuckEditCommandConfig(),
            //        };
            //    }
            //    else
            //    {
            //        throw new NotImplementedException();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _dialogService.ShowError(ex.Message);
            //}

            //UpdateComboColumnState(SelectedColumn);

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
        private void CompleteColumns()
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
        }

        private void SqlServerPopulateColumns(SqlServerDataAdapterConfig adapterConfig)
        {
            if (adapterConfig.DataProvider is not SqlServerDataProviderConfig dataProviderConfig) return;

            var query = dataProviderConfig.Text;

            if (string.IsNullOrWhiteSpace(query)) return;

            var requestedParameters = ParameterParser.ParseQueryParameters(query);
            List<SqlParameter> sqlParams = [];

            foreach (var queryParam in requestedParameters)
            {
                if (string.IsNullOrEmpty(queryParam.Name))
                    continue;

                var safeParamName = $"@{queryParam.Name.Replace(".", "_")}";

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

            using (var conn = new SqlConnection(connectionString))
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

            var connectionString = adapterConfig.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _configService.Load().Settings.DuckDbConnectionString;
            }

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
    }
}
