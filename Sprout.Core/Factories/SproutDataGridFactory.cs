using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.UIStates;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.Factories
{
    public class SproutDataGridFactory : BaseSproutControlFactory
    {
        internal static SproutDataGrid GenerateSproutGrid(SproutDataGridConfig sproutGridConfig, Dictionary<string, UIElement> controls)
        {
            var sproutDataGrid = new SproutDataGrid
            {
                Name = sproutGridConfig.Name,
                Config = sproutGridConfig,
            };

            foreach (var colConfig in sproutGridConfig.Columns ?? [])
            {
                DataGridColumn col = null;

                if (colConfig.ColumnType == ColumnType.Check)
                {
                    col = new DataGridCheckBoxColumn()
                    {
                        Header = colConfig.Header,
                        Binding = new Binding(colConfig.BindingPath),
                        Width = DataGridLength.Auto,
                        IsReadOnly = colConfig.IsReadOnly
                    };
                }
                else if (colConfig.ColumnType == ColumnType.Combo)
                {
                    var comboCol = new DataGridComboBoxColumn()
                    {
                        Header = colConfig.Header,
                        DisplayMemberPath = colConfig.DisplayColumn,
                        SelectedValuePath = colConfig.ValueColumn,
                        SelectedValueBinding = new Binding(colConfig.BindingPath),
                        Width = DataGridLength.Auto,
                        IsReadOnly = colConfig.IsReadOnly
                    };

                    // Bind to the DataContext of the DataGrid itself
                    Binding vmBinding = new ($"DataContext.DataProviders[{colConfig.ComboAdapterKey}].Data")
                    {
                        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1)
                    };

                    var style = new Style(typeof(ComboBox));
                    style.Setters.Add(new Setter(ComboBox.ItemsSourceProperty, vmBinding));

                    comboCol.ElementStyle = style;
                    comboCol.EditingElementStyle = style;

                    col = comboCol;
                }
                else
                {
                    col = new DataGridTextColumn()
                    {
                        Header = colConfig.Header,
                        Binding = new Binding(colConfig.BindingPath),
                        Width = DataGridLength.Auto,
                        IsReadOnly = colConfig.IsReadOnly
                    };
                }

                sproutDataGrid.dataGrid.Columns.Add(col);
            }

            AddControl(sproutDataGrid, controls);

            SetPositionInGrid(sproutDataGrid, sproutGridConfig);

            var gridUIState = new SproutGridUIState();
            gridUIState.SetUpState(sproutDataGrid);

            return sproutDataGrid;
        }
    }
}
