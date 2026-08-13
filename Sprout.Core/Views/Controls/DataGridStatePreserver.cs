using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Preserves a <see cref="DataGrid"/>'s scroll position and selected row across
    /// ItemsSource (DataTable) replacements, e.g. when the data is refreshed.
    /// <see cref="Capture"/> must be called just before the data is replaced
    /// (e.g. from the VM's data provider change notification, which runs before the
    /// ItemsSource binding updates); the restore is scheduled to run after the new data lands.
    /// Selection is restored by primary key when available, otherwise by an unambiguous
    /// full-row value match, falling back to the selected index.
    /// </summary>
    public sealed class DataGridStatePreserver
    {
        private readonly DataGrid _dataGrid;

        private double _savedVerticalOffset;
        private double _savedHorizontalOffset;
        private System.Data.DataRowView _lastSelectedRow;
        private int _savedSelectedIndex = -1;
        private int _captureVersion;

        public DataGridStatePreserver(DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
        }

        /// <summary>
        /// Captures the current scroll position and selected row, and schedules their
        /// restore for after the new data has been rendered. Call right before the grid's data is replaced.
        /// </summary>
        public void Capture()
        {
            var scrollViewer = FindScrollViewer(_dataGrid);
            _savedVerticalOffset = scrollViewer?.VerticalOffset ?? 0;
            _savedHorizontalOffset = scrollViewer?.HorizontalOffset ?? 0;

            _lastSelectedRow = _dataGrid.SelectedItem as System.Data.DataRowView;
            _savedSelectedIndex = _dataGrid.SelectedIndex;

            //If refreshes overlap, only the restore matching the latest capture may run.
            var version = ++_captureVersion;

            //Deferred at Loaded priority: runs after the ItemsSource binding has pushed
            //the new table into the grid and row virtualization has re-measured.
            _dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (version != _captureVersion) return;

                RestoreSelection();

                //Release the reference to the old row: keeping it would root the entire
                //replaced DataTable and prevent it from being garbage collected.
                _lastSelectedRow = null;

                var sv = FindScrollViewer(_dataGrid);
                if (sv != null)
                {
                    sv.ScrollToVerticalOffset(_savedVerticalOffset);
                    sv.ScrollToHorizontalOffset(_savedHorizontalOffset);
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void RestoreSelection()
        {
            //ItemsSource is bound to a DataTable; depending on how the binding resolves it,
            //the actual value can be either the DataView or the DataTable itself.
            var dv = _dataGrid.ItemsSource switch
            {
                System.Data.DataView view => view,
                System.Data.DataTable table => table.DefaultView,
                _ => null
            };

            var oldRow = _lastSelectedRow?.Row;

            //A row that was deleted or committed away since the capture is Detached/Deleted:
            //its values can no longer be read, so fall through to the index restore.
            if (oldRow != null &&
                (oldRow.RowState == System.Data.DataRowState.Detached ||
                 oldRow.RowState == System.Data.DataRowState.Deleted))
            {
                oldRow = null;
            }

            //1. Restore selection by primary key (row instances are new after a refresh).
            if (dv != null && oldRow != null && dv.Table.PrimaryKey.Length > 0 &&
                oldRow.Table.PrimaryKey.Length == dv.Table.PrimaryKey.Length)
            {
                var key = oldRow.Table.PrimaryKey.Select(pk => oldRow[pk]).ToArray();
                var row = dv.Table.Rows.Find(key);
                if (row != null)
                {
                    _dataGrid.SelectedItem = dv.Cast<System.Data.DataRowView>()
                                               .FirstOrDefault(rowView => rowView.Row == row);
                    if (_dataGrid.SelectedItem != null)
                    {
                        _dataGrid.ScrollIntoView(_dataGrid.SelectedItem);
                        return;
                    }
                }
            }

            //2. No primary key: match on the full set of column values. Only select when the match is unambiguous.
            if (dv != null && oldRow != null && dv.Table.PrimaryKey.Length == 0)
            {
                var matches = dv.Cast<System.Data.DataRowView>()
                    .Where(rowView => oldRow.Table.Columns
                        .Cast<System.Data.DataColumn>()
                        .All(c => rowView.Row.Table.Columns.Contains(c.ColumnName) &&
                                  Equals(rowView.Row[c.ColumnName], oldRow[c])))
                    .Take(2)
                    .ToList();

                if (matches.Count == 1)
                {
                    _dataGrid.SelectedItem = matches[0];
                    _dataGrid.ScrollIntoView(matches[0]);
                    return;
                }
            }

            //3. Last resort: restore by index (clamped to the new row count).
            if (_savedSelectedIndex >= 0 && _dataGrid.Items.Count > 0)
            {
                _dataGrid.SelectedIndex = Math.Min(_savedSelectedIndex, _dataGrid.Items.Count - 1);
            }
        }

        private static ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var result = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (result != null) return result;
            }
            return null;
        }
    }
}
