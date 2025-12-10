using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sprout.Core.Services
{
    public class LegacyGridGeneratorService
    {
        /// <summary>
        /// Generates a Grid control based on JSON configuration.
        /// </summary>
        /// <param name="jsonConfiguration">The JSON configuration string.</param>
        /// <returns>A configured Grid control.</returns>
        public Grid GenerateGrid(string jsonConfiguration)
        {
            var gridConfig = JsonSerializer.Deserialize<GridConfiguration>(jsonConfiguration);

            if (gridConfig?.Grid == null)
            {
                throw new ArgumentException("Invalid grid configuration JSON.");
            }

            var grid = new Grid();
            //grid.ShowGridLines = true;

            // Add rows
            foreach (var row in gridConfig.Grid.Rows ?? new List<string>())
            {
                var rowDef = new RowDefinition { Height = ParseGridLength(row) };
                grid.RowDefinitions.Add(rowDef);
            }

            // Add columns
            foreach (var column in gridConfig.Grid.Columns ?? new List<string>())
            {
                var colDef = new ColumnDefinition { Width = ParseGridLength(column) };
                grid.ColumnDefinitions.Add(colDef);
            }

            // Add items to the grid
            foreach (var item in gridConfig.Grid.Items ?? new List<GridItem>())
            {
                var element = CreateGridElement(item);
                if (element != null)
                {
                    AddElementToGrid(grid, element, item);
                }
            }

            return grid;
        }

        /// <summary>
        /// Parses a grid length string (e.g., "1*", "Auto", "100").
        /// </summary>
        private System.Windows.GridLength ParseGridLength(string lengthString)
        {
            if (string.IsNullOrWhiteSpace(lengthString))
            {
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            }

            lengthString = lengthString.Trim();

            if (lengthString.EndsWith("*"))
            {
                var numberPart = lengthString.Substring(0, lengthString.Length - 1);
                if (double.TryParse(numberPart, out var value))
                {
                    return new System.Windows.GridLength(value, System.Windows.GridUnitType.Star);
                }
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            }

            if (lengthString.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto);
            }

            if (double.TryParse(lengthString, out var pixelValue))
            {
                return new System.Windows.GridLength(pixelValue, System.Windows.GridUnitType.Pixel);
            }

            return new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
        }

        /// <summary>
        /// Creates a control element based on the item configuration.
        /// </summary>
        private UIElement CreateGridElement(GridItem item)
        {
            return item.Type?.ToLower() switch
            {
                "data-grid" => CreateDataGrid(item),
                "text" => CreateTextBlock(item),
                "button" => CreateButton(item),
                "testcontrol" => CreateTestControl(item),
                _ => null
            };
        }

        private TestControl CreateTestControl(GridItem item)
        {
            return new TestControl();
        }

        public DataGrid DataGrid { get; set; }

        /// <summary>
        /// Creates a DataGrid control with the specified columns.
        /// </summary>
        private DataGrid CreateDataGrid(GridItem item)
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true
            };



            DataGrid = dataGrid;

            if (item.Margin > 0)
            {
                dataGrid.Margin = new Thickness(item.Margin);
            }

            // Add columns to the DataGrid
            if (item.Columns != null)
            {
                foreach (var column in item.Columns)
                {
                    var dataGridColumn = CreateDataGridColumn(column);
                    if (dataGridColumn != null)
                    {
                        dataGrid.Columns.Add(dataGridColumn);
                    }
                }
            }

            // Store reference to item for later data binding
            dataGrid.Tag = item;

            return dataGrid;
        }

        /// <summary>
        /// Creates a DataGrid column based on the column configuration.
        /// </summary>
        private DataGridColumn CreateDataGridColumn(GridColumn column)
        {
            return column.Type?.ToLower() switch
            {
                "text" => new DataGridTextColumn
                {
                    Header = column.Header,
                    Binding = new System.Windows.Data.Binding(column.Field)
                },
                "number" => new DataGridTextColumn
                {
                    Header = column.Header,
                    Binding = new System.Windows.Data.Binding(column.Field)
                },
                _ => null
            };
        }

        /// <summary>
        /// Creates a TextBlock control.
        /// </summary>
        private TextBlock CreateTextBlock(GridItem item)
        {
            return new TextBlock
            {
                Text = item.Content ?? string.Empty,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
        }

        /// <summary>
        /// Creates a Button control.
        /// </summary>
        private Button CreateButton(GridItem item)
        {
            return new Button
            {
                Content = item.Content ?? "Button",
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
        }

        /// <summary>
        /// Adds an element to the grid at the specified position.
        /// </summary>
        private void AddElementToGrid(Grid grid, UIElement element, GridItem item)
        {
            // Set grid position (1-based indexing converted to 0-based)
            var row = Math.Max(0, (item.Row ?? 1) - 1);
            var column = Math.Max(0, (item.Column ?? 1) - 1);

            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);

            // Set row and column spans if specified
            if (item.RowSpan.HasValue)
            {
                Grid.SetRowSpan(element, item.RowSpan.Value);
            }

            if (item.ColumnSpan.HasValue)
            {
                Grid.SetColumnSpan(element, item.ColumnSpan.Value);
            }

            grid.Children.Add(element);
        }

        /// <summary>
        /// Binds data to all DataGrid controls in the grid.
        /// </summary>
        public void BindDataToGrid(Grid grid, List<Dictionary<string, object>> data)
        {
            // Find all DataGrid controls in the grid
            FindAndBindDataGrids(grid, data);
        }

        private void FindAndBindDataGrids(DependencyObject parent, List<Dictionary<string, object>> data)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is DataGrid dataGrid)
                {
                    // Bind the data
                    dataGrid.ItemsSource = DictionaryDataSource.CreateBindableSource(data);
                }

                // Recursively check children
                FindAndBindDataGrids(child, data);
            }
        }

        /// <summary>
        /// Configuration classes for JSON deserialization.
        /// </summary>
        private class GridConfiguration
        {
            [JsonPropertyName("grid")]
            public GridDefinition Grid { get; set; }
        }

        private class GridDefinition
        {
            [JsonPropertyName("rows")]
            public List<string> Rows { get; set; }

            [JsonPropertyName("columns")]
            public List<string> Columns { get; set; }

            [JsonPropertyName("items")]
            public List<GridItem> Items { get; set; }
        }

        private class GridItem
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("row")]
            public int? Row { get; set; }

            [JsonPropertyName("column")]
            public int? Column { get; set; }

            [JsonPropertyName("row-span")]
            public int? RowSpan { get; set; }

            [JsonPropertyName("column-span")]
            public int? ColumnSpan { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("columns")]
            public List<GridColumn> Columns { get; set; }

            [JsonPropertyName("margin")]
            public int Margin { get; set; }
        }

        private class GridColumn
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("header")]
            public string Header { get; set; }

            [JsonPropertyName("field")]
            public string Field { get; set; }
        }
    }
}
