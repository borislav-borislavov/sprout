using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class RefreshDataGridButtonAction : IButtonAction
    {
        private readonly string _targetDataGridName;

        public RefreshDataGridButtonAction(string targetDataGridName)
        {
            _targetDataGridName = targetDataGridName;
        }

        public async Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            var targetDataAdapter = vmRegistry.GetAdapterOrThrow(_targetDataGridName);

            using (var dataService = dataServiceFactory.Create(targetDataAdapter, vmRegistry))
            {
                await dataService.ProvideData();
            }
        }
    }
}
