using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.DataAdapters.Filters
{
    public class SqlServerFilter : IFilter
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public object StartValue { get; set; }

        public object EndValue { get; set; }
        public bool IsRange { get; set; }
    }
}
