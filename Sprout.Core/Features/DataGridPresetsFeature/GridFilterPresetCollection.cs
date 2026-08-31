using Sprout.Core.Models.Configurations.DataGrid;
using System.Collections.Generic;

namespace Sprout.Core.Features.DataGridPresetsFeature
{
    /// <summary>
    /// All saved filter presets of a single grid on a single page,
    /// persisted as one ValueStore entry.
    /// </summary>
    public class GridFilterPresetCollection
    {
        /// <summary>
        /// Preset name -> saved preset (filter values and column layout).
        /// </summary>
        public Dictionary<string, GridFilterPreset> Presets { get; set; } = [];

        /// <summary>
        /// The preset applied automatically on page start. Null when no default is set.
        /// </summary>
        public string SelectedPresetName { get; set; }
    }

    /// <summary>
    /// A single named preset: the saved filter values plus the grid's column settings
    /// (order, visibility and frozen count) captured when the preset was saved.
    /// </summary>
    public class GridFilterPreset
    {
        /// <summary>
        /// Filter title -> saved values.
        /// </summary>
        public Dictionary<string, FilterValueState> Filters { get; set; } = [];

        /// <summary>
        /// The grid's column layout at the time the preset was saved. Null for presets
        /// saved before layouts were captured.
        /// </summary>
        public SproutGridColumnLayout ColumnLayout { get; set; }
    }

    /// <summary>
    /// The saved state of a single filter.
    /// </summary>
    public class FilterValueState
    {
        public string Start { get; set; }

        public string End { get; set; }
    }
}
