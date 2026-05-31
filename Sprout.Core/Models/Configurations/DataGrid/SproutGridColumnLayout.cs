using System.Collections.Generic;

namespace Sprout.Core.Models.Configurations.DataGrid
{
    /// <summary>
    /// Persisted user customization of a <see cref="SproutDataGridConfig"/>'s columns:
    /// their order, visibility and the number of frozen (locked) leading columns.
    /// </summary>
    public class SproutGridColumnLayout
    {
        /// <summary>
        /// The number of leading columns that should be frozen (locked) so they stay
        /// visible while scrolling horizontally.
        /// </summary>
        public int FrozenColumnCount { get; set; }

        /// <summary>
        /// The per-column state, in the order the columns should be displayed.
        /// </summary>
        public List<SproutGridColumnState> Columns { get; set; } = [];
    }

    public class SproutGridColumnState
    {
        /// <summary>
        /// Stable identifier of the column (its binding path).
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Whether the column is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
