using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;

namespace Sprout.Core.Features.ButtonActions.Actions
{
    public class CompositeButtonAction : IButtonAction, IButtonActionMessenger
    {
        private readonly List<IButtonAction> _actions = [];

        public List<ActionMessage> Messages { get; } = [];

        public void Add(IButtonAction action)
        {
            _actions.Add(action);
        }

        public async Task Perform(Dictionary<string, IDataAdapter> dataAdapters, VMRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            ResetMessages();

            foreach (var action in _actions)
            {
                await action.Perform(dataAdapters, uiStateRegistry, dataServiceFactory);

                if (action is IButtonActionMessenger messenger && messenger.Messages.Any())
                    Messages.AddRange(messenger.Messages);
            }
        }

        public void ResetMessages()
        {
            Messages.Clear();
        }
    }
}
