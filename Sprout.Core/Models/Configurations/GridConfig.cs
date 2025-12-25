using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class GridConfig : SproutControlConfig
    {
        public List<string> Rows { get; set; } = [];

        public List<string> Columns { get; set; } = [];

        public List<SproutControlConfig> Children { get; set; } = [];
    }
}
