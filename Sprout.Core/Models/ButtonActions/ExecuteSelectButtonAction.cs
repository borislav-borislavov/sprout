using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.GridActions;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.ButtonActions
{
    public class ExecuteSelectButtonAction : IButtonAction
    {
        private readonly string _ownControlName;

        public ExecuteSelectButtonAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public async Task Perform(Dictionary<string, IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                throw new Exception($"DataAdapter not found for control '{_ownControlName}'");
            }

            using (var dataService = dataServiceFactory.Create(ownDataAdapter, uiStateRegistry))
            {
                await dataService.ProvideData();
            }

            var buttonState = uiStateRegistry.Get<SproutButtonUIState>(_ownControlName);

            if (buttonState != null && ownDataAdapter.DataProvider?.Data != null)
            {
                var table = ownDataAdapter.DataProvider.Data;
                buttonState.FirstRow = table.DefaultView.Count > 0 ? table.DefaultView[0] : null;
            }
        }
    }
}
