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
using System.Windows.Controls.Primitives;
using System.Windows.Media;

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

            foreach (var colConfig in (sproutGridConfig.Columns ?? []).Where(c => !c.ShowInRowDetails))
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
                sproutDataGrid.ColumnKeys[col] = colConfig.BindingPath ?? colConfig.Header;
            }

            var rowDetailColumns = (sproutGridConfig.Columns ?? []).Where(c => c.ShowInRowDetails).ToList();
            if (rowDetailColumns.Count > 0)
            {
                sproutDataGrid.dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                sproutDataGrid.dataGrid.RowDetailsTemplate = BuildRowDetailsTemplate(rowDetailColumns, sproutGridConfig.RowDetailsItemsPerRow);
            }

            AddControl(sproutDataGrid, controls);

            SetPositionInGrid(sproutDataGrid, sproutGridConfig);

            var gridUIState = new SproutGridUIState();
            gridUIState.SetUpState(sproutDataGrid);

            return sproutDataGrid;
        }

        private static DataTemplate BuildRowDetailsTemplate(List<SproutDataGridColumnConfig> columns, int itemsPerRow)
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(245, 245, 245)));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Colors.LightGray));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
            border.SetValue(Border.PaddingProperty, new Thickness(8, 4, 8, 4));

            int columns_ = Math.Max(1, itemsPerRow);

            var uniformGrid = new FrameworkElementFactory(typeof(UniformGrid));
            uniformGrid.SetValue(UniformGrid.ColumnsProperty, columns_);

            foreach (var col in columns)
            {
                var entryPanel = new FrameworkElementFactory(typeof(StackPanel));
                entryPanel.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
                entryPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 2, 16, 2));

                var header = new FrameworkElementFactory(typeof(TextBlock));
                header.SetValue(TextBlock.TextProperty, (col.Header ?? col.BindingPath) + ": ");
                header.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
                header.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.DarkSlateGray));

                var value = new FrameworkElementFactory(typeof(TextBlock));
                value.SetBinding(TextBlock.TextProperty, new Binding(col.BindingPath));
                value.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.DarkSlateGray));

                entryPanel.AppendChild(header);
                entryPanel.AppendChild(value);
                uniformGrid.AppendChild(entryPanel);
            }

            border.AppendChild(uniformGrid);

            return new DataTemplate { VisualTree = border };
        }
    }
}
