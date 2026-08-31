using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Common
{
    public static class Const
    {
        public const string AppName = "Sprout";
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

        public const string LogFileName = "SproutLog.txt";

        ///When this file is created inside the seed vault, the app will always ask for a seed file to be selected instead of using the default main.seed file.
        public const string AlwaysAsk = "AlwaysAsk.txt";

        public const string DefaultSeedFileName = "main.seed";
    }
}
