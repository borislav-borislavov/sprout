using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters.DataProviders
{
    public partial class SqlServerDataProvider : ObservableObject, IDataProvider
    {
        public string Name { get; set; }

        /// <summary>
        /// The Root table name of the query. Used for automatic updates.
        /// </summary>
        public string TableName { get; set; }
        public List<string> PrimaryKeys { get; set; } = [];
        public string Text { get; set; }

        [ObservableProperty]
        private DataTable _data;

        public Dictionary<string, IFilter> Filters { get; set; } = [];

        private SqlServerDataAdapter _parentAdapter;

        public SqlServerDataProvider(SqlServerDataAdapter parentAdapter)
        {
            _parentAdapter = parentAdapter;
        }

        public string ConnectionString => _parentAdapter.ConnectionString;


        public IEnumerable<DataProviderDependency> Dependencies { get; internal set; } = [];
    }
}
