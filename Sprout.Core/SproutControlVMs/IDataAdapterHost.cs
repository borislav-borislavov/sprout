using System;
using System.Collections.Generic;
using System.Text;
using IDataAdapter = Sprout.Core.Models.DataAdapters.IDataAdapter;

namespace Sprout.Core.SproutControlVMs
{
    public interface IDataAdapterHost
    {
        IDataAdapter DataAdapter { get; set; }
    }
}
