using Sprout.Core.Models.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.GridActions
{
    public class AddRowGridAction : GridAction
    {
        private readonly string gridQueryName;

        public AddRowGridAction(string gridQueryName)
        {
            this.gridQueryName = gridQueryName;
        }

        public override void Perform(Dictionary<string, IDataProvider> dataProviders)
        {
            if (!dataProviders.TryGetValue(gridQueryName, out var ownQuery))
            {
                //find a nice way to route logs to the screen

                throw new NotImplementedException();
            }

            var newRow = ownQuery.Data.NewRow();
            ownQuery.Data.Rows.Add(newRow);
        }
    }
}
