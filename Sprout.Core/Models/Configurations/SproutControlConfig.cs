using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public abstract class SproutControlConfig
    {
        public int Row { get; set; }
        public int RowSpan { get; set; }

        public int Column { get; set; }
        public int ColumnSpan { get; set; }
    }
}
