using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class GridConfig : SproutControlConfig
    {
        public List<string> Rows { get; set; } = [];

        public List<string> Columns { get; set; } = [];

        public bool ShowGridLines { get; set; }

        public ObservableCollection<SproutControlConfig> Children { get; set; } = [];
    }
}
