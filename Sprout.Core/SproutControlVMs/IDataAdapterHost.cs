using Sprout.Core.Models.DataAdapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Sprout.Core.SproutControlVMs
{
    public interface IDataAdapterHost
    {
        IDataAdapter DataAdapter { get; set; }
    }

    public interface IDataAdapterDictionaryHost
    {
        Dictionary<string, IDataAdapter> DataAdapters { get; set; }
    }
}
