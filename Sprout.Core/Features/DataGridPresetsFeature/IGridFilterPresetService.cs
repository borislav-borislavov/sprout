using System;
using System.Collections.Generic;

namespace Sprout.Core.Features.DataGridPresetsFeature
{
    /// <summary>
    /// Persists SproutDataGrid filter presets in the ValueStore.
    /// Presets are scoped by page and grid; each preset is a named snapshot of filter values.
    /// </summary>
    public interface IGridFilterPresetService
    {
        /// <summary>
        /// Returns the presets of the given grid. Never returns null.
        /// </summary>
        GridFilterPresetCollection GetCollection(Guid pageId, string gridName);

        /// <summary>
        /// Adds or overwrites a preset and persists it.
        /// </summary>
        void SavePreset(Guid pageId, string gridName, string presetName, GridFilterPreset preset);

        /// <summary>
        /// Removes a preset. Clears the default when it pointed to the removed preset.
        /// Returns true when the preset existed.
        /// </summary>
        bool DeletePreset(Guid pageId, string gridName, string presetName);

        /// <summary>
        /// Marks a preset as the default applied on page start. Pass null to clear the default.
        /// </summary>
        void SetDefaultPreset(Guid pageId, string gridName, string presetName);
    }
}
