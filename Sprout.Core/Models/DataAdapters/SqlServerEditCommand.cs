using Sprout.Core.Models.DataAdapters.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters
{
	public class SqlServerEditCommand : IEditCommand
	{
        private SqlServerDataAdapter _parentAdapter;

        public SqlServerEditCommand(SqlServerDataAdapter parentDataAdapter)
        {
            _parentAdapter = parentDataAdapter;
        }

        public string Text { get; set; }
        public string ConnectionString => _parentAdapter.ConnectionString;
    }
}
