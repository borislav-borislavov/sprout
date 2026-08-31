using Sprout.Core.Features.DataGridPresetsFeature;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Services.ValueStore;
using System;
using System.IO;

namespace Sprout.Tests.Unit
{
    public class GridFilterPresetServiceTests : IDisposable
    {
        private readonly string _tempPath;
        private readonly GridFilterPresetService _service;
        private readonly Guid _pageId = Guid.NewGuid();
        private const string GridName = "dgTest";

        public GridFilterPresetServiceTests()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), "SproutTests_" + Guid.NewGuid());
            _service = new GridFilterPresetService(new ValueStoreFactory(_tempPath));
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempPath))
                Directory.Delete(_tempPath, recursive: true);
        }

        private static GridFilterPreset SampleValues(string start = "alice")
            => new()
            {
                Filters = new()
                {
                    ["UserName"] = new FilterValueState { Start = start },
                    ["Created"] = new FilterValueState { Start = "2024-01-01", End = "2024-12-31" }
                }
            };

        [Fact]
        public void SavePreset_RoundTripsValues()
        {
            _service.SavePreset(_pageId, GridName, "My Preset", SampleValues());

            var collection = _service.GetCollection(_pageId, GridName);

            Assert.True(collection.Presets.ContainsKey("My Preset"));
            Assert.Equal("alice", collection.Presets["My Preset"].Filters["UserName"].Start);
            Assert.Equal("2024-12-31", collection.Presets["My Preset"].Filters["Created"].End);
        }

        [Fact]
        public void SavePreset_RoundTripsColumnLayout()
        {
            var preset = SampleValues();
            preset.ColumnLayout = new SproutGridColumnLayout
            {
                FrozenColumnCount = 2,
                Columns =
                [
                    new SproutGridColumnState { Key = "UserName", IsVisible = true, Width = 175.5 },
                    new SproutGridColumnState { Key = "Created", IsVisible = false, Width = 240 }
                ]
            };

            _service.SavePreset(_pageId, GridName, "My Preset", preset);

            var layout = _service.GetCollection(_pageId, GridName).Presets["My Preset"].ColumnLayout;

            Assert.NotNull(layout);
            Assert.Equal(2, layout.FrozenColumnCount);
            Assert.Equal(["UserName", "Created"], layout.Columns.Select(c => c.Key));
            Assert.False(layout.Columns[1].IsVisible);
            Assert.Equal(175.5, layout.Columns[0].Width);
            Assert.Equal(240, layout.Columns[1].Width);
        }

        [Fact]
        public void SavePreset_OverwritesExistingPreset()
        {
            _service.SavePreset(_pageId, GridName, "My Preset", SampleValues("alice"));
            _service.SavePreset(_pageId, GridName, "My Preset", SampleValues("bob"));

            var collection = _service.GetCollection(_pageId, GridName);

            Assert.Single(collection.Presets);
            Assert.Equal("bob", collection.Presets["My Preset"].Filters["UserName"].Start);
        }

        [Fact]
        public void GetCollection_UnknownGrid_ReturnsEmptyCollection()
        {
            var collection = _service.GetCollection(_pageId, "unknown");

            Assert.NotNull(collection);
            Assert.Empty(collection.Presets);
            Assert.Null(collection.SelectedPresetName);
        }

        [Fact]
        public void SetDefaultPreset_PersistsDefault()
        {
            _service.SavePreset(_pageId, GridName, "My Preset", SampleValues());

            _service.SetDefaultPreset(_pageId, GridName, "My Preset");

            Assert.Equal("My Preset", _service.GetCollection(_pageId, GridName).SelectedPresetName);
        }

        [Fact]
        public void SetDefaultPreset_UnknownPreset_Throws()
        {
            Assert.Throws<ArgumentException>(() => _service.SetDefaultPreset(_pageId, GridName, "missing"));
        }

        [Fact]
        public void DeletePreset_RemovesPresetAndClearsDefault()
        {
            _service.SavePreset(_pageId, GridName, "My Preset", SampleValues());
            _service.SetDefaultPreset(_pageId, GridName, "My Preset");

            var removed = _service.DeletePreset(_pageId, GridName, "My Preset");

            var collection = _service.GetCollection(_pageId, GridName);
            Assert.True(removed);
            Assert.Empty(collection.Presets);
            Assert.Null(collection.SelectedPresetName);
        }

        [Fact]
        public void DeletePreset_UnknownPreset_ReturnsFalse()
        {
            Assert.False(_service.DeletePreset(_pageId, GridName, "missing"));
        }

        [Fact]
        public void Presets_AreScopedByPageAndGrid()
        {
            var otherPageId = Guid.NewGuid();

            _service.SavePreset(_pageId, GridName, "My Preset", SampleValues("alice"));
            _service.SavePreset(otherPageId, GridName, "My Preset", SampleValues("bob"));
            _service.SavePreset(_pageId, "otherGrid", "My Preset", SampleValues("carol"));

            Assert.Equal("alice", _service.GetCollection(_pageId, GridName).Presets["My Preset"].Filters["UserName"].Start);
            Assert.Equal("bob", _service.GetCollection(otherPageId, GridName).Presets["My Preset"].Filters["UserName"].Start);
            Assert.Equal("carol", _service.GetCollection(_pageId, "otherGrid").Presets["My Preset"].Filters["UserName"].Start);
        }
    }
}
