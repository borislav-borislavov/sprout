using Sprout.Core.Factories;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Features.ButtonActions
{
    public interface IButtonAction
    {
        public abstract Task Perform(Dictionary<string, IDataAdapter> dataAdapters, VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory);
    }
}
