using Sprout.Core.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Factories
{
    public class DataTableFactory
    {
        public static DataTable Create()
        {
            var dt = new DataTable();

            var isDeletedCol = dt.Columns.Add(Const.BuiltInDataTableColumns._IsDeleted, typeof(bool));
            isDeletedCol.DefaultValue = false;

            dt.Columns.Add(Const.BuiltInDataTableColumns._RowBackColor, typeof(string));

            return dt;
        }
    }
}
