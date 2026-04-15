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
    public class CompositeButtonAction : IButtonAction, IButtonActionMessenger
    {
        private readonly List<IButtonAction> _actions = [];

        public List<ActionMessage> Messages { get; } = [];

        public void Add(IButtonAction action)
        {
            _actions.Add(action);
        }

        public async Task Perform(Dictionary<string, IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
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
