using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sprout.Core.Models.Configurations
{
    /// <summary>
    /// A purely visual tab control that hosts a child layout per tab.
    /// </summary>
    public class SproutTabControlConfig : SproutControlConfig
    {
        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }

        public ObservableCollection<SproutTabItemConfig> Tabs { get; set; } = [];

        /// <summary>
        /// Exposes the tabs for tree-view binding.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SproutControlConfig> ChildCollection => Tabs;
    }

    /// <summary>
    /// A single tab of a <see cref="SproutTabControlConfig"/>. Hosts one child layout.
    /// </summary>
    public class SproutTabItemConfig : SproutControlConfig, IChildControlHost
    {
        public string Header { get; set; } = "Tab";

        public SproutControlConfig Child { get; set; } = new GridConfig { Name = "TabGrid", Children = [] };

        /// <summary>
        /// Wraps the single Child in a collection for tree-view binding.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SproutControlConfig> ChildCollection => Child != null ? [Child] : [];
    }
}
