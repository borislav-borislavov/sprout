using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.DataGrid
{
    public class SproutDataGridColumnConfig
    {
        public string Header { get; set; }

        /// <summary>
        /// The DataTable.ColumnName to which this column will be bound
        /// </summary>
        public string BindingPath { get; set; }

        public ColumnType ColumnType { get; set; }
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// The column name from the ComboBox data source used for display text.
        /// Only applicable when ColumnType is Combo.
        /// </summary>
        public string DisplayColumn { get; set; }

        /// <summary>
        /// The column name from the ComboBox data source used as the selected value.
        /// Only applicable when ColumnType is Combo.
        /// </summary>
        public string ValueColumn { get; set; }

        /// <summary>
        /// The data adapter configuration that provides the items source for the ComboBox.
        /// Only applicable when ColumnType is Combo. This adapter only needs a DataProvider (read-only).
        /// </summary>
        public IDataAdapterConfig ComboDataAdapter { get; set; }

        public string ComboAdapterKey;
    }

    public enum ColumnType
    {
        Text,
        Check,
        Combo,
        Date,
        DateTime
    }
}
