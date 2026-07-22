using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.Dependency;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class ExecuteSelectButtonAction : IButtonAction
    {
        private readonly string _ownControlName;

        public ExecuteSelectButtonAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public async Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var ownDataAdapter = vmRegistry.GetAdapterOrThrow(_ownControlName);

            //The DataProvider dependencies of buttons are intentionally skipped to provide a more intuitive behavior of the control.
            //The code below refreshes the values of the dependencies manually to provide up to date data.
            foreach (var dependency in ownDataAdapter.DataProvider.Dependencies)
            {
                var targetedControlVM = vmRegistry[dependency.ControlName];

                if (targetedControlVM == null)
                    throw new Exception($"VM for control {dependency.ControlName} not found.");

                dependency.Value = BindingEvaluator.Evaluate(targetedControlVM, dependency.PropertyPath);
            }

            using (var dataService = dataServiceFactory.Create(ownDataAdapter, vmRegistry))
            {
                await dataService.ProvideData();
            }

            var buttonState = vmRegistry.Get<SproutButtonVM>(_ownControlName);

            if (buttonState != null && ownDataAdapter.DataProvider?.Data != null)
            {
                var table = ownDataAdapter.DataProvider.Data;
                buttonState.FirstRow = table.DefaultView.Count > 0 ? table.DefaultView[0] : null;
            }
        }
    }
}
