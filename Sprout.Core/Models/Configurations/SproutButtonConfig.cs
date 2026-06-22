using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SproutButtonConfig : SproutControlConfig, IDataAdapterConfigHost
    {
        public string Content { get; set; }

        public string Icon { get; set; }

        public string IconFont { get; set; } = "Segoe MDL2 Assets";

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }

        public string Padding { get; set; }

        public int BrushThickness { get; set; } = 1;

        public IDataAdapterConfig DataAdapter { get; set; }

        public ObservableCollection<SproutButtonActionConfig> Actions { get; set; } = [];
    }
}
