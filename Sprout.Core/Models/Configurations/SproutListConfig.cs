using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Sprout.Core.Models.Configurations
{
    /// <summary>
    /// A data bound list control. It hosts a single <see cref="Child"/> control which is used as the
    /// template for every item the list displays. The items themselves are provided by the
    /// <see cref="DataAdapter"/>. Supports an optional border, a header/title, a footer that shows the
    /// number of items and a built in client side search box.
    /// </summary>
    public class SproutListConfig : SproutControlConfig, IChildControlHost, IDataAdapterConfigHost
    {
        /// <summary>
        /// The title displayed in the header of the list.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// When true the list is wrapped in a styled border.
        /// </summary>
        public bool ShowBorder { get; set; } = true;

        public string Background { get; set; }

        public string BorderBrush { get; set; } = "#2C3E50";

        public double BorderThickness { get; set; } = 1;

        public double CornerRadius { get; set; }

        /// <summary>
        /// When true the footer that displays the number of items is shown.
        /// </summary>
        public bool ShowFooter { get; set; } = true;

        /// <summary>
        /// When true a search box is shown in the header allowing the user to filter the items.
        /// </summary>
        public bool ShowSearch { get; set; } = true;

        /// <summary>
        /// Message shown when the list has no items to display.
        /// </summary>
        public string EmptyText { get; set; } = "No items to display";

        public double? Height { get; set; }

        public double? Width { get; set; }

        public string Margin { get; set; }

        public string Padding { get; set; }

        public string HorizontalAlignment { get; set; }

        public string VerticalAlignment { get; set; }

        public string ToolTip { get; set; }

        /// <summary>
        /// The data source that provides the items rendered by the list.
        /// </summary>
        public IDataAdapterConfig DataAdapter { get; set; }

        /// <summary>
        /// The pages that the list can open from its page-launch menu. When one or more pages are
        /// configured a button is shown beneath the header; selecting a page opens it and passes the
        /// list's currently selected item as the page parameter.
        /// </summary>
        public ObservableCollection<SproutListPageLink> Pages { get; set; } = [];

        /// <summary>
        /// The control used to build the template for each item displayed in the list.
        /// </summary>
        public SproutControlConfig Child { get; set; } = new GridConfig { Name = "ListItemTemplate", Children = [] };

        /// <summary>
        /// Wraps the single Child in a collection for tree-view binding.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SproutControlConfig> ChildCollection => Child != null ? [Child] : [];
    }
}
