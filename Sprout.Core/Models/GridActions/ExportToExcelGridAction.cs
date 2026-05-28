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
            for (int col = 0; col < data.Columns.Count; col++)
            {
                var columnName = data.Columns[col].ColumnName;
                if (columnName.StartsWith("_"))
                    continue;

                worksheet.Cell(1, excelCol).Value = columnName;
                excelCol++;
            }

            // Add data rows
            int rowIndex = 2;
            foreach (DataRow row in data.Rows)
            {
                excelCol = 1;
                for (int col = 0; col < data.Columns.Count; col++)
                {
                    var columnName = data.Columns[col].ColumnName;
                    if (columnName.StartsWith("_"))
                        continue;

                    var value = row[col];
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
    }
}
