using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    /// <summary>
    /// This interface is tied to the IDataProvider and in general to the DataProvider notion which queries will be refactored at a later time.
    /// </summary>
    public interface IDataRetreiver
    {
        string QueryName { get; set; }
    }
}
