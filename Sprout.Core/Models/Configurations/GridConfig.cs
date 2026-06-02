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
        public ObservableCollection<StringItem> Rows { get; set; } = [];

        public ObservableCollection<StringItem> Columns { get; set; } = [];

        public bool ShowGridLines { get; set; }

        public string Background { get; set; }

        public string Margin { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }

        public ObservableCollection<SproutControlConfig> Children { get; set; } = [];
    }
}
