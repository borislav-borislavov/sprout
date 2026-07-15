using Sprout.Core.Factories;
using Sprout.Core.Features.Dependency;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.UIStates;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class ExecuteSelectButtonAction : IButtonAction
    {
        private readonly string _ownControlName;

        public ExecuteSelectButtonAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public async Task Perform(Dictionary<string, IDataAdapter> dataAdapters, VMRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                throw new Exception($"DataAdapter not found for control '{_ownControlName}'");
            }

            //The DataProvider dependencies of buttons are intentionally skipped to provide a more intuitive behavior of the control.
            //The code below refreshes the values of the dependencies manually to provide up to date data.
            foreach (var dependency in ownDataAdapter.DataProvider.Dependencies)
            {
                var targetedControlUIState = uiStateRegistry[dependency.ControlName];

                if (targetedControlUIState == null)
                    throw new Exception($"UI State for control {dependency.ControlName} not found.");

                dependency.Value = BindingEvaluator.Evaluate(targetedControlUIState, dependency.PropertyPath);
            }

            using (var dataService = dataServiceFactory.Create(ownDataAdapter, uiStateRegistry))
            {
                await dataService.ProvideData();
            }

            var buttonState = uiStateRegistry.Get<SproutButtonVM>(_ownControlName);

            if (buttonState != null && ownDataAdapter.DataProvider?.Data != null)
            {
                var table = ownDataAdapter.DataProvider.Data;
                buttonState.FirstRow = table.DefaultView.Count > 0 ? table.DefaultView[0] : null;
            }
        }
    }
}
