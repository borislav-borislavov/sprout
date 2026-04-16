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

        /// <summary>
        /// Designed to apply DataTable fixes after the data is loaded.
        /// </summary>
        internal static void PostLoadLogic(DataTable dt)
        {
            foreach (DataColumn column in dt.Columns)
            {
                //if a collumn does not allow nulls, when creating a new row the app throws an error
                //this code fixes this so that it is handled via sql
                column.AllowDBNull = true;

                //This prevents the DataTable from automatically assigning PK values
                if (column.AutoIncrement)
                {
                    column.AutoIncrement = false;
                }
            }
        }
    }
}
