using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SproutComboConfig : SproutControlConfig, IDataRetreiver
    {
        public string QueryName { get; set; }
        public string DisplayColumn { get; set; }
        public string ValueColumn { get; set; }

        public string VerticalAlignment { get; set; }
    }
}
