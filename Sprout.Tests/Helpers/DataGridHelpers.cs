using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace Sprout.Tests.Helpers
{
    internal static class DataGridHelpers
    {
        internal static string ComboDisplayText(this DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            var cell = dataGrid.Columns[columnIndex].GetCellContent(row); // returns the element (TextBlock in display mode, or ComboBox in edit mode)

            if (cell is ComboBox cmb)
            {
                if (cmb.SelectedItem == null)
                    return null;

                // Resolve the display member the same way WPF binding does (via TypeDescriptor),
                // so items like DataRowView that use ICustomTypeDescriptor also work.
                var displayValue = TypeDescriptor.GetProperties(cmb.SelectedItem)[cmb.DisplayMemberPath]
                    ?.GetValue(cmb.SelectedItem);

                return displayValue?.ToString();
            }
            else
            {
                throw new Exception("Cell is not a ComboBox.");
            }
        }
    }
}
