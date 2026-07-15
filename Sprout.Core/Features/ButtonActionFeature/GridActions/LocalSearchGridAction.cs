using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.UIStates;
using Sprout.Core.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    /// <summary>
    /// Performs a Local Search: filters the grid's currently loaded data (client-side)
    /// by applying a <see cref="DataView.RowFilter"/> that searches every column for a
    /// value the user enters in a small prompt. No data is fetched from the database.
    /// </summary>
    public class LocalSearchGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        private string _lastSearchText = string.Empty;

        private static readonly string[] _excludedColumns =
        [
            Const.BuiltInDataTableColumns._IsDeleted,
            Const.BuiltInDataTableColumns._RowBackColor
        ];

        public LocalSearchGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public Task Perform(Dictionary<string, Models.DataAdapters.IDataAdapter> dataAdapters, VMRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            var gridUiState = uiStateRegistry.Get<SproutDataGridVM>(_ownControlName);

            if (gridUiState == null)
                throw new Exception($"Failed to find SproutDataGridUIState for {_ownControlName}");

            if (gridUiState.Grid?.dataGrid?.ItemsSource is not DataView dataView)
                return Task.CompletedTask;

            var prompt = new LocalSearchWindow(_lastSearchText)
            {
                Owner = Window.GetWindow(gridUiState.Grid)
            };

            if (prompt.ShowDialog() != true)
                return Task.CompletedTask;

            _lastSearchText = prompt.SearchText ?? string.Empty;

            dataView.RowFilter = BuildRowFilter(dataView.Table, _lastSearchText);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds a RowFilter expression that matches the search text against every
        /// searchable column. Returns an empty string (no filter) when the search
        /// text is empty.
        /// </summary>
        private static string BuildRowFilter(DataTable table, string searchText)
        {
            if (table == null || string.IsNullOrWhiteSpace(searchText))
                return string.Empty;

            var likeValue = EscapeLikeValue(searchText.Trim());

            var clauses = table.Columns
                .Cast<DataColumn>()
                .Where(c => !_excludedColumns.Contains(c.ColumnName))
                .Select(c => $"CONVERT([{EscapeColumnName(c.ColumnName)}], 'System.String') LIKE '%{likeValue}%'");

            var filter = string.Join(" OR ", clauses);

            return filter;
        }

        /// <summary>
        /// Escapes a column name for use inside square brackets in a RowFilter expression.
        /// </summary>
        private static string EscapeColumnName(string columnName)
            => columnName.Replace("]", "\\]");

        /// <summary>
        /// Escapes a value for use inside a LIKE clause: doubles single quotes and
        /// wraps the LIKE wildcard characters so they are treated literally.
        /// </summary>
        private static string EscapeLikeValue(string value)
        {
            var sb = new StringBuilder(value.Length);

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\'':
                        sb.Append("''");
                        break;
                    case '%':
                    case '*':
                    case '[':
                    case ']':
                        sb.Append('[').Append(ch).Append(']');
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
