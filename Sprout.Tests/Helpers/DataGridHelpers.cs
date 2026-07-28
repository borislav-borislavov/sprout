using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            else if (dataGrid.Columns[columnIndex] is DataGridTemplateColumn templateCol 
                && templateCol.CellTemplate.VisualTree.Type == typeof(ComboBox))
            {
                FrameworkElement cellContent = templateCol.GetCellContent(row);
                ComboBox comboBox = DataGridHelpers.FindVisualChild<ComboBox>(cellContent);

                if (comboBox.SelectedItem == null)
                    return null;

                // Resolve the display member the same way WPF binding does (via TypeDescriptor),
                // so items like DataRowView that use ICustomTypeDescriptor also work.
                var displayValue = TypeDescriptor.GetProperties(comboBox.SelectedItem)[comboBox.DisplayMemberPath]?.GetValue(comboBox.SelectedItem);

                return displayValue?.ToString();
            }
            else
            {
                throw new Exception("Cell is not a ComboBox.");
            }
        }


        internal static ComboBox GetComboBox(this DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            if (dataGrid.Columns[columnIndex] is DataGridTemplateColumn templateCol
                && templateCol.CellTemplate.VisualTree.Type == typeof(ComboBox))
            {
                FrameworkElement cellContent = templateCol.GetCellContent(row);
                ComboBox comboBox = DataGridHelpers.FindVisualChild<ComboBox>(cellContent);

                return comboBox;
            }
            else
            {
                throw new Exception("Cell is not a ComboBox.");
            }
        }

        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
