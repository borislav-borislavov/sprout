using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.GridActions
{
    public class AddRowGridAction : GridAction
    {
        private readonly string _ownControlName;

        public AddRowGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public override void Perform(Dictionary<string, Sprout.Core.Models.DataAdapters.IDataAdapter> dataProviders)
        {
            if (!dataProviders.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                //find a nice way to route logs to the screen

                throw new NotImplementedException();
            }

            var newRow = ownDataAdapter.DataProvider.Data.NewRow();
            ownDataAdapter.DataProvider.Data.Rows.Add(newRow);
        }
    }
}
