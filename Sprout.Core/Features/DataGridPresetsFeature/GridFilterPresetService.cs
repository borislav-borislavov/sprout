using Sprout.Core.Services.ValueStore;
using System;
using System.Collections.Generic;

namespace Sprout.Core.Features.DataGridPresetsFeature
{
    public class GridFilterPresetService : IGridFilterPresetService
    {
        private const string Category = "GridFilterPresets";

        private readonly IValueStoreFactory _valueStoreFactory;

        public GridFilterPresetService(IValueStoreFactory valueStoreFactory)
        {
            _valueStoreFactory = valueStoreFactory;
        }

        public GridFilterPresetCollection GetCollection(Guid pageId, string gridName)
        {
            var store = _valueStoreFactory.Get(Category);

            return store.Get<GridFilterPresetCollection>(GetKey(pageId, gridName)) ?? new GridFilterPresetCollection();
        }

        public void SavePreset(Guid pageId, string gridName, string presetName, GridFilterPreset preset)
        {
            if (string.IsNullOrWhiteSpace(presetName))
                throw new ArgumentException("Preset name cannot be null or empty.", nameof(presetName));

            var collection = GetCollection(pageId, gridName);
            collection.Presets[presetName] = preset ?? new GridFilterPreset();

            Save(pageId, gridName, collection);
        }

        public bool DeletePreset(Guid pageId, string gridName, string presetName)
        {
            var collection = GetCollection(pageId, gridName);

            if (!collection.Presets.Remove(presetName))
                return false;

            if (collection.SelectedPresetName == presetName)
                collection.SelectedPresetName = null;

            Save(pageId, gridName, collection);
            return true;
        }

        public void SetDefaultPreset(Guid pageId, string gridName, string presetName)
        {
            var collection = GetCollection(pageId, gridName);

            if (presetName != null && !collection.Presets.ContainsKey(presetName))
                throw new ArgumentException($"Preset '{presetName}' does not exist for grid '{gridName}'.", nameof(presetName));

            collection.SelectedPresetName = presetName;

            Save(pageId, gridName, collection);
        }

        private void Save(Guid pageId, string gridName, GridFilterPresetCollection collection)
        {
            _valueStoreFactory.Get(Category).Save(GetKey(pageId, gridName), collection);
        }

        private static string GetKey(Guid pageId, string gridName) => $"{pageId}.{gridName}";
    }
}
