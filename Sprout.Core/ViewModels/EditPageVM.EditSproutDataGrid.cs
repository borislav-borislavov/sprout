using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Queries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //[ObservableProperty]
        //private SproutPageConfiguration _selectedNonMenuPage;

        partial void OnSelectedColumnChanged(SproutDataGridColumnConfig value)
        {
            UpdateComboColumnState(value);
        }

        private void UpdateComboColumnState(SproutDataGridColumnConfig column)
        {
            if (column != null && column.ColumnType == ColumnType.Combo)
            {
                IsComboColumnSelected = true;

                if (column.ComboDataAdapter is SqlServerDataAdapterConfig sqlConfig)
                {
                    SelectedColumnAdapterViewModel = new SqlServerReadOnlyDataAdapterVM(sqlConfig);
                }
                else
                {
                    SelectedColumnAdapterViewModel = null;
                }
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

            if (SelectedColumn.ComboDataAdapter != null) return;

            SelectedColumn.ComboDataAdapter = new SqlServerDataAdapterConfig
            {
                ConnectionString = string.Empty,
                DataProvider = new SqlServerDataProviderConfig
                {
                    Text = string.Empty
                }
            };

            UpdateComboColumnState(SelectedColumn);
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
#warning this should be put in a factory which repsponds properly based on the type of adapter
            if (SelectedDataGrid.DataAdapter is SqlServerDataAdapterConfig adapterConfig)
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

                using (var conn = new SqlConnection(adapterConfig.ConnectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    try
                    {
                        AttachParameters(cmd, sqlParams);

                        conn.Open();

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
}
