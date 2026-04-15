using Microsoft.Data.SqlClient;
using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.ButtonActions
{
    public class ExecuteUpdateButtonAction : IButtonAction, IButtonActionMessenger
    {
        private readonly string _ownControlName;

        public List<ActionMessage> Messages { get; private set; } = [];

        public ExecuteUpdateButtonAction(string ownControlName)
        {
            _ownControlName = ownControlName;
        }

        public async Task Perform(Dictionary<string, DataAdapters.IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            ResetMessages();

            if (!dataAdapters.TryGetValue(_ownControlName, out var ownDataAdapter))
            {
                throw new Exception($"DataAdapter not found for control '{_ownControlName}'");
            }

            if (ownDataAdapter.UpdateCommand is not SqlServerEditCommand sqlEditCommand)
            {
                throw new Exception($"UpdateCommand is not configured for control '{_ownControlName}'");
            }

            using (var dataService = dataServiceFactory.Create(ownDataAdapter, uiStateRegistry))
            {
                var messages = await dataService.Update(null);

                if (messages.Any())
                    Messages.AddRange(messages);
            }
        }

        public void ResetMessages()
        {
            Messages.Clear();
        }
    }
}
