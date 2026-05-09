using Sprout.Core.Models.DataAdapters.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters
{
    public class DuckEditCommand : IEditCommand
    {
        private DuckDataAdapter _parentAdapter;

        public string Text { get; set; }
        public string ConnectionString => _parentAdapter.ConnectionString;

        /// <summary>
        /// When true additional code is added to Insert/Update/Delete queries which allows for messages to be piped out of these operations.
        /// </summary>
        public bool WithMessages { get; set; } = true;

        public DuckEditCommand(DuckDataAdapter parentDataAdapter)
        {
            _parentAdapter = parentDataAdapter;
        }
    }
}