using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.SproutControlVMs;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class ExecuteUpdateButtonAction : IButtonAction, IButtonActionMessenger
    {
        private readonly string _ownControlName;

        public List<ActionMessage> Messages { get; private set; } = [];

        public ExecuteUpdateButtonAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public async Task Perform(Dictionary<string, Models.DataAdapters.IDataAdapter> dataAdapters, VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
        {
            ResetMessages();

            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                throw new Exception($"DataAdapter not found for control '{_ownControlName}'");
            }

            using (var dataService = dataServiceFactory.Create(ownDataAdapter, vmRegistry))
            {
                var changeResult = await dataService.Update(null);

                if (changeResult.Messages.Any())
                    Messages.AddRange(changeResult.Messages);
            }
        }

        public void ResetMessages()
        {
            Messages.Clear();
        }
    }
}
