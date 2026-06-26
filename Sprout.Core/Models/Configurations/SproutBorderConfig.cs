using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Sprout.Core.Models.Configurations
{
    public class SproutBorderConfig : SproutControlConfig, IChildControlHost
    {
        public string Background { get; set; }

        public string BorderBrush { get; set; } = "#2C3E50";

        public double BorderThickness { get; set; } = 1;

        public double CornerRadius { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        public string Padding { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }

        public SproutControlConfig Child { get; set; } = new GridConfig { Name = "BorderGrid", Children = [] };

        /// <summary>
        /// Wraps the single Child in a collection for tree-view binding.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SproutControlConfig> ChildCollection => Child != null ? [Child] : [];
    }
}
