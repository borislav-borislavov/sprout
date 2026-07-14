using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Common;
using Sprout.Core.Models.Configurations.DataGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Sprout.Core.ViewModels
{
    /// <summary>
    /// Lightweight, read-only view model that projects a single grid row into a
    /// vertical list of ColumnName / Value pairs and supports live filtering by column name.
    /// </summary>
    public partial class RowPreviewVM : ObservableObject
    {
        private static readonly string[] _hiddenColumns =
            [Const.BuiltInDataTableColumns._IsDeleted, Const.BuiltInDataTableColumns._RowBackColor];

        [ObservableProperty]
        private ObservableCollection<RowPreviewItemVM> _items = [];

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    RefreshFilter();
            }
        }

        private string _valueSearchText = string.Empty;
        public string ValueSearchText
        {
            get => _valueSearchText;
            set
            {
                if (SetProperty(ref _valueSearchText, value))
                    RefreshFilter();
            }
        }

        private bool _caseSensitive;
        public bool CaseSensitive
        {
            get => _caseSensitive;
            set
            {
                if (SetProperty(ref _caseSensitive, value))
                    RefreshFilter();
            }
        }

        public ICollectionView FilteredItems { get; private set; }

        private int _filteredCount;
        public int FilteredCount
        {
            get => _filteredCount;
            private set => SetProperty(ref _filteredCount, value);
        }

        private string _windowTitle = "Row preview";
        public string WindowTitle
        {
            get => _windowTitle;
            private set => SetProperty(ref _windowTitle, value);
        }

        private void RefreshFilter()
        {
            FilteredItems?.Refresh();
            FilteredCount = FilteredItems?.Cast<object>().Count() ?? 0;
        }

        private readonly Services.Clipboard.IClipboardService _clipboardService;

        public RowPreviewVM(DataRowView row, IEnumerable<SproutDataGridColumnConfig> columns, Services.Clipboard.IClipboardService clipboardService)
        {
            _clipboardService = clipboardService;
            Load(row, columns);
        }

        private void Load(DataRowView row, IEnumerable<SproutDataGridColumnConfig> columns)
        {
            Items.Clear();

            if (row?.Row != null)
            {
                foreach (var (columnName, bindingPath) in ResolveColumns(row.Row, columns))
                {
                    if (!row.Row.Table.Columns.Contains(bindingPath))
                        continue;

                    Items.Add(new RowPreviewItemVM(columnName, FormatValue(row.Row[bindingPath])));
                }
            }

            FilteredItems = CollectionViewSource.GetDefaultView(Items);
            FilteredItems.Filter = o => o is RowPreviewItemVM item && MatchesFilter(item);
            OnPropertyChanged(nameof(FilteredItems));
            FilteredCount = FilteredItems.Cast<object>().Count();

            var firstValue = Items.FirstOrDefault()?.Value;
            WindowTitle = string.IsNullOrWhiteSpace(firstValue) ? "Row preview" : $"Row preview: {firstValue}";
        }

        private bool MatchesFilter(RowPreviewItemVM item)
        {
            var valueComparison = _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            bool matchesColumn = string.IsNullOrWhiteSpace(_searchText) ||
                (item.ColumnName?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false);

            bool matchesValue = string.IsNullOrWhiteSpace(_valueSearchText) ||
                (item.Value?.Contains(_valueSearchText, valueComparison) ?? false);

            return matchesColumn && matchesValue;
        }

        /// <summary>
        /// Determines which columns to preview. Uses the grid's configured columns
        /// (respecting headers and skipping row-detail columns) when available,
        /// otherwise falls back to the raw table columns.
        /// </summary>
        private static IEnumerable<(string ColumnName, string BindingPath)> ResolveColumns(
            DataRow row, IEnumerable<SproutDataGridColumnConfig> columns)
        {
            var configured = (columns ?? [])
                .Where(c => !c.ShowInRowDetails && !string.IsNullOrEmpty(c.BindingPath))
                .ToList();

            if (configured.Count > 0)
            {
                foreach (var col in configured)
                    yield return (string.IsNullOrEmpty(col.Header) ? col.BindingPath : col.Header, col.BindingPath);

                yield break;
            }

            foreach (DataColumn col in row.Table.Columns)
            {
                if (_hiddenColumns.Contains(col.ColumnName))
                    continue;

                yield return (col.ColumnName, col.ColumnName);
            }
        }

        private static string FormatValue(object value)
            => value == null || value == DBNull.Value ? string.Empty : value.ToString();

        [RelayCommand]
        private void ClearSearch() => SearchText = string.Empty;

        [RelayCommand]
        private void ClearValueSearch() => ValueSearchText = string.Empty;

        [RelayCommand]
        private void CopyFilteredResults()
        {
            if (_clipboardService == null || FilteredItems == null)
                return;

            var sb = new StringBuilder();
            foreach (var item in FilteredItems.Cast<RowPreviewItemVM>())
                sb.AppendLine($"{item.ColumnName}: {item.Value}");

            _clipboardService.SetText(sb.ToString());
        }
    }

    /// <summary>
    /// Read-only ColumnName / Value pair displayed in the row preview window.
    /// </summary>
    public class RowPreviewItemVM
    {
        public RowPreviewItemVM(string columnName, string value)
        {
            ColumnName = columnName;
            Value = value;
        }

        public string ColumnName { get; }

        public string Value { get; }
    }
}
