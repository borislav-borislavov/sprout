using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.GridActions
{
    public class CompositeButtonAction : GridAction
    {
        private readonly List<GridAction> _actions = [];

        public void Add(GridAction action)
        {
            _actions.Add(action);
        }

        public override async Task Perform(Dictionary<string, IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory)
        {
            foreach (var action in _actions)
            {
                await action.Perform(dataAdapters, uiStateRegistry, dataServiceFactory);
            }
        }
    }
}
