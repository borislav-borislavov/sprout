using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.GridActions
{
    internal class RefreshDataGridAction : GridAction
    {
        private readonly string _ownControlName;

        public RefreshDataGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public override void Perform(Dictionary<string, IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry)
        {
            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                //find a nice way to route logs to the screen

                throw new NotImplementedException();
            }

            new DataProviderService().ProvideData(ownDataAdapter.DataProvider);
        }
    }
}
