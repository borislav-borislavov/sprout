using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Common
{
    public static class Const
    {
        public static class BuiltInDataTableColumns
        {
            public const string _IsDeleted = nameof(BuiltInDataTableColumns._IsDeleted);
            public const string _RowBackColor = nameof(BuiltInDataTableColumns._RowBackColor);
        }

        public static class SqlServer
        {
            public const string WhereFilter = "{!whereFilter}";
            public const string AndFilter = "{!andFilter}";
        }
    }
}
