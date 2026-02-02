using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters.Filters
{
    public interface IFilter
    {
        string Title { get; set; }

        string Text { get; set; }

        object StartValue { get; set; }

        object EndValue { get; set; }
        bool IsRange { get; set;}
    }
}
