using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Common
{
    public static class Const
    {
        /// <summary>
        /// This is the keyword by which login information is fetched.
        /// Ex.: {@Login.User.UserID}
        /// </summary>
        public const string Login = "Login";
        
        /// <summary>
        /// This is the keyword by which a parent grid data  passes values to a detail page.
        /// Ex.: {@Page.Data.SomeID}
        /// </summary>
        public const string Page = "Page";

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
