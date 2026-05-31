using Microsoft.VisualBasic;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Queries;
using Sprout.Core.UIStates;
using Sprout.Core.ViewModels;
using Sprout.Core.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutDataGrid.xaml
    /// </summary>
    public partial class SproutDataGrid : UserControl
    {
        public SproutDataGridConfig Config { get; set; }

        /// <summary>
        /// Maps each generated <see cref="DataGridColumn"/> to a stable key (its binding path)
        /// so column layout can be persisted and re-applied across sessions.
        /// </summary>
        public Dictionary<DataGridColumn, string> ColumnKeys { get; } = [];

        public static readonly DependencyProperty UIStateProperty =
            DependencyProperty.Register(nameof(UIState), typeof(SproutGridUIState), typeof(SproutDataGrid), new PropertyMetadata(null));

        public SproutGridUIState UIState
        {
            get => (SproutGridUIState)GetValue(UIStateProperty);
            set => SetValue(UIStateProperty, value);
        }

        public SproutDataGrid()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Returns the stable key (binding path) for the given column.
        /// </summary>
        public string GetColumnKey(DataGridColumn column)
            => ColumnKeys.TryGetValue(column, out var key) ? key : column?.Header?.ToString();

        /// <summary>
        /// Applies a persisted column layout (visibility, order and frozen count) to the grid.
        /// </summary>
        public void ApplyColumnLayout(SproutGridColumnLayout layout)
        {
            if (layout == null) return;

            foreach (var col in dataGrid.Columns)
            {
                var state = layout.Columns.FirstOrDefault(c => c.Key == GetColumnKey(col));
                if (state != null)
                {
                    col.Visibility = state.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            int displayIndex = 0;
            foreach (var state in layout.Columns)
            {
                var col = dataGrid.Columns.FirstOrDefault(c => GetColumnKey(c) == state.Key);
                if (col != null && displayIndex < dataGrid.Columns.Count)
                {
                    col.DisplayIndex = displayIndex++;
                }
            }

            dataGrid.FrozenColumnCount = Math.Max(0, Math.Min(layout.FrozenColumnCount, dataGrid.Columns.Count));
        }

        /// <summary>
        /// Returns the keys (binding paths) of the currently visible columns, in their display order.
        /// Reflects the user's show/hide and reorder customizations.
        /// </summary>
        public List<string> GetVisibleColumnKeysInDisplayOrder()
            => dataGrid.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .OrderBy(c => c.DisplayIndex)
                .Select(GetColumnKey)
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

        /// <summary>
        /// Builds a layout snapshot describing the grid's current column visibility, order and frozen count.
        /// </summary>
        public SproutGridColumnLayout GetCurrentLayout()
            => new()
            {
                FrozenColumnCount = dataGrid.FrozenColumnCount,
                Columns = dataGrid.Columns
                    .OrderBy(c => c.DisplayIndex)
                    .Select(c => new SproutGridColumnState
                    {
                        Key = GetColumnKey(c),
                        IsVisible = c.Visibility == Visibility.Visible
                    })
                    .ToList()
            };

        /// <summary>
        /// Builds the default layout from the grid's configuration: all columns visible,
        /// in their original order, with no frozen columns.
        /// </summary>
        public SproutGridColumnLayout GetDefaultLayout()
            => new()
            {
                FrozenColumnCount = 0,
                Columns = (Config?.Columns ?? [])
                    .Select(c => new SproutGridColumnState
                    {
                        Key = c.BindingPath ?? c.Header,
                        IsVisible = true
                    })
                    .ToList()
            };

        private void btnColumnSettings_Click(object sender, RoutedEventArgs e)
        {
            var vm = new ColumnSettingsVM(UIState);
            var window = new ColumnSettings(vm)
            {
                Owner = Window.GetWindow(this)
            };

            window.ShowDialog();
        }

        private void btnFilters_Click(object sender, RoutedEventArgs e)
        {
            if(dataGrid.Visibility == Visibility.Visible)
            {
                dataGrid.Visibility = Visibility.Collapsed;
                filtersBorder.Visibility = Visibility.Visible;
                FiltersGrid.Visibility = Visibility.Visible;

                filtersButtonPanel.Visibility = Visibility.Visible;
                gridButtonsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowDataGrid();
            }   
        }

        private void btnApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            ShowDataGrid();
        }

        private void ShowDataGrid()
        {
            dataGrid.Visibility = Visibility.Visible;
            filtersBorder.Visibility = Visibility.Collapsed;
            FiltersGrid.Visibility = Visibility.Collapsed;
            filtersButtonPanel.Visibility = Visibility.Collapsed;
            gridButtonsPanel.Visibility = Visibility.Visible;
        }
    }
}
