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

        public async Task Perform(Dictionary<string, IDataAdapter> dataAdapters, VMRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            if (!dataAdapters.TryGetValue(_targetDataGridName, out var targetDataAdapter))
            {
                throw new Exception($"DataGrid '{_targetDataGridName}' does not exist, grid refresh failed.");
            }

            using (var dataService = dataServiceFactory.Create(targetDataAdapter, uiStateRegistry))
            {
                await dataService.ProvideData();
            }
        }
    }
}
