using Sprout.Core.Models.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.GridActions
{
    public abstract class GridAction
    {
        public abstract void Perform(Dictionary<string, IDataProvider> dataProviders);
    }
}
