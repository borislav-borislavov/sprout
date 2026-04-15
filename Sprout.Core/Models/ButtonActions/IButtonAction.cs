using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.ButtonActions
{
    public interface IButtonAction
    {
        public abstract Task Perform(Dictionary<string, IDataAdapter> dataAdapters, UiStateRegistry uiStateRegistry, IDataServiceFactory dataServiceFactory);
    }
}
