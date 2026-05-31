using ClosedXML.Excel;
using Microsoft.Win32;
using Sprout.Core.Factories;
using Sprout.Core.Models.ButtonActions;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataTable = System.Data.DataTable;
using DataRow = System.Data.DataRow;

namespace Sprout.Core.Models.GridActions
{
    public class ExportToExcelGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        public ExportToExcelGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public Task Perform(Dictionary<string, Sprout.Core.Models.DataAdapters.IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                throw new NotImplementedException();
            }

            var data = ownDataAdapter.DataProvider.Data;

            if (data == null || data.Rows.Count == 0)
                return Task.CompletedTask;

            // Respect the user's column settings (visibility and order) coming from the grid.
            var orderedColumnNames = GetExportColumnNames(data, uiStateRegistry);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = $"{_ownControlName}_Export"
            };

            if (saveFileDialog.ShowDialog() != true)
                return Task.CompletedTask;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Export");

            // Add headers
            int excelCol = 1;
            foreach (var columnName in orderedColumnNames)
            {
                worksheet.Cell(1, excelCol).Value = columnName;
                excelCol++;
            }

            // Add data rows
            int rowIndex = 2;
            foreach (DataRow row in data.Rows)
            {
                excelCol = 1;
                foreach (var columnName in orderedColumnNames)
                {
                    var value = row[columnName];
                    if (value != null && value != DBNull.Value)
                    {
                        worksheet.Cell(rowIndex, excelCol).Value = value.ToString();
                    }
                    excelCol++;
                }
                rowIndex++;
            }

            var range = worksheet.Range(1, 1, rowIndex - 1, excelCol - 1);
            range.CreateTable();

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(saveFileDialog.FileName);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Determines which DataTable columns to export and in what order, honoring the
        /// grid's column settings (visibility and display order) when available.
        /// Falls back to all non-internal columns in their natural order.
        /// </summary>
        private List<string> GetExportColumnNames(DataTable data, UiStateRegistry uiStateRegistry)
        {
            var availableColumns = new List<string>();
            foreach (System.Data.DataColumn column in data.Columns)
            {
                if (!column.ColumnName.StartsWith("_"))
                    availableColumns.Add(column.ColumnName);
            }

            var gridState = uiStateRegistry.Get<SproutGridUIState>(_ownControlName);
            var visibleKeys = gridState?.Grid?.GetVisibleColumnKeysInDisplayOrder();

            if (visibleKeys == null || visibleKeys.Count == 0)
                return availableColumns;

            var ordered = new List<string>();
            foreach (var key in visibleKeys)
            {
                if (data.Columns.Contains(key) && !key.StartsWith("_"))
                    ordered.Add(key);
            }

            return ordered.Count > 0 ? ordered : availableColumns;
        }
    }
}
