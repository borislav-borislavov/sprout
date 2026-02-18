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
