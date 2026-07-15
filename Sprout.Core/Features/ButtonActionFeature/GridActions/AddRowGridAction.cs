using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions.GridActions
{
    public class AddRowGridAction : IButtonAction
    {
        private readonly string _ownControlName;

        public AddRowGridAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public Task Perform(Dictionary<string, Models.DataAdapters.IDataAdapter> dataAdapters,
            UiStateRegistry uiStateRegistry,
            IDataServiceFactory dataServiceFactory)
        {
            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                //find a nice way to route logs to the screen

                throw new NotImplementedException();
            }

            var newRow = ownDataAdapter.DataProvider.Data.NewRow();
            ownDataAdapter.DataProvider.Data.Rows.Add(newRow);

            return Task.CompletedTask;
        }
    }
}
