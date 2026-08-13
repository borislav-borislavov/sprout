using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.Factories
{
    public class SproutDataGridColumnFactory
    {
        private readonly IDataAdapterFactory _dataAdapterFactory;
        private readonly SproutDataGrid _sproutDataGrid;

        public SproutDataGridColumnFactory(IDataAdapterFactory dataAdapterFactory, SproutDataGrid sproutDataGrid)
        {
            _dataAdapterFactory = dataAdapterFactory;
            _sproutDataGrid = sproutDataGrid;
        }

        public DataGridColumn Create(SproutDataGridColumnConfig colConfig)
        {
            switch (colConfig.ColumnType)
            {
                case ColumnType.Text:
                    return CreateTextBox(colConfig);
                case ColumnType.Check:
                    return CreateCheckBox(colConfig);
                case ColumnType.Combo:
                    return CreateComboBox(colConfig);
                case ColumnType.Date:
                    return CreateDatePicker(colConfig);
                case ColumnType.DateTime:
                    return CreateDateTimePicker(colConfig);
                default:
                    throw new NotImplementedException();
            }
        }

        private DataGridColumn CreateCheckBox(SproutDataGridColumnConfig colConfig)
        {
            var createTemplated = true;

            if (createTemplated)
            {
                // 1. Create the template column
                var templateCol = new DataGridTemplateColumn()
                {
                    Header = colConfig.Header,
                    Width = GetWidth(colConfig),
                    IsReadOnly = colConfig.IsReadOnly
                };

                // 2. Build the CheckBox factory for the template
                var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
                checkBoxFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                checkBoxFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                checkBoxFactory.SetValue(UIElement.IsEnabledProperty, !colConfig.IsReadOnly);

                // 3. Create the binding with PropertyChanged so it commits instantly on click
                var binding = new Binding(colConfig.BindingPath)
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                // 4. Bind the CheckBox's IsChecked property to your data source
                checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, binding);

                // 5. Assign the visual template to the column
                templateCol.CellTemplate = new DataTemplate { VisualTree = checkBoxFactory };

                return templateCol;
            }
            else
            {
                var col = new DataGridCheckBoxColumn()
                {
                    Header = colConfig.Header,
                    Binding = new Binding(colConfig.BindingPath),
                    Width = GetWidth(colConfig),
                    IsReadOnly = colConfig.IsReadOnly
                };

                return col;
            }
        }

        private DataGridColumn CreateComboBox(SproutDataGridColumnConfig colConfig)
        {
            var createTemplated = true;

            if (createTemplated)
            {
                // Create the DataAdapter to which the ComboBox will bind
                _sproutDataGrid.VM.DataAdapters.Add(colConfig.ComboAdapterKey, _dataAdapterFactory.Create(colConfig.DataAdapter));

                var templateColumn = new DataGridTemplateColumn()
                {
                    Header = colConfig.Header,
                    Width = GetWidth(colConfig),
                    IsReadOnly = colConfig.IsReadOnly
                };

                // 1. Create the factory for the ComboBox
                var comboFactory = new FrameworkElementFactory(typeof(ComboBox));

                // 2. Map the Display and Value paths
                comboFactory.SetValue(ItemsControl.DisplayMemberPathProperty, colConfig.DisplayColumn);
                comboFactory.SetValue(System.Windows.Controls.Primitives.Selector.SelectedValuePathProperty, colConfig.ValueColumn);

                // 3. Create the ItemsSource binding (replaces your old BindingOperations.SetBinding)
                var itemsSourceBinding = new Binding()
                {
                    Source = _sproutDataGrid.VM.DataAdapters[colConfig.ComboAdapterKey].DataProvider,
                    Path = new PropertyPath("Data")
                };
                comboFactory.SetBinding(ItemsControl.ItemsSourceProperty, itemsSourceBinding);

                // 4. Create the SelectedValue binding with instant commit (PropertyChanged)
                var selectedValueBinding = new Binding(colConfig.BindingPath)
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = colConfig.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
                };
                comboFactory.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedValueProperty, selectedValueBinding);

                // 5. Respect the IsReadOnly configuration
                if (colConfig.IsReadOnly)
                {
                    comboFactory.SetValue(UIElement.IsEnabledProperty, false);
                }

                // 6. Assign the visual tree to the CellTemplate
                templateColumn.CellTemplate = new DataTemplate { VisualTree = comboFactory };

                return templateColumn;
            }
            else
            {
                //Create the DataAdapter to which the ComboBox will bind
                _sproutDataGrid.VM.DataAdapters.Add(colConfig.ComboAdapterKey, _dataAdapterFactory.Create(colConfig.DataAdapter));

                var comboCol = new DataGridComboBoxColumn()
                {
                    Header = colConfig.Header,
                    DisplayMemberPath = colConfig.DisplayColumn,
                    SelectedValuePath = colConfig.ValueColumn,
                    SelectedValueBinding = new Binding(colConfig.BindingPath),
                    Width = GetWidth(colConfig),
                    IsReadOnly = colConfig.IsReadOnly
                };

                var vmBinding = new Binding()
                {
                    //Bind directly to the DataProvider instance (a plain Dictionary raises no
                    //change notifications). DataTable is not IEnumerable, so use its DefaultView.
                    Source = _sproutDataGrid.VM.DataAdapters[colConfig.ComboAdapterKey].DataProvider,
                    Path = new PropertyPath("Data")
                };

                //Bind the column's own ItemsSource so both the display and editing
                //elements get their items and can resolve the selected value.
                BindingOperations.SetBinding(comboCol, DataGridComboBoxColumn.ItemsSourceProperty, vmBinding);

                return comboCol;
            }
        }

        private static DataGridColumn CreateDateTimePicker(SproutDataGridColumnConfig colConfig)
        {
            throw new NotImplementedException();
        }

        private static DataGridColumn CreateDatePicker(SproutDataGridColumnConfig colConfig)
        {
            throw new NotImplementedException();
        }

        private static DataGridColumn CreateTextBox(SproutDataGridColumnConfig colConfig)
        {
            var col = new DataGridTextColumn()
            {
                Header = colConfig.Header,
                Binding = new Binding(colConfig.BindingPath),
                Width = GetWidth(colConfig),
                IsReadOnly = colConfig.IsReadOnly
            };

            return col;
        }

        private static DataGridLength GetWidth(SproutDataGridColumnConfig colConfig)
        {
            return colConfig.Width.HasValue
                ? new DataGridLength(colConfig.Width.Value)
                : DataGridLength.Auto;
        }
    }
}
