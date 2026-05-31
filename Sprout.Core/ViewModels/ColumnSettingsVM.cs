using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Views.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.ViewModels
{
    public partial class ColumnSettingsVM : ObservableObject
    {
        private readonly SproutDataGrid _grid;
        private readonly IConfigurationService _configurationService;

        [ObservableProperty]
        private ObservableCollection<ColumnSettingItemVM> _columns = [];

        [ObservableProperty]
        private ColumnSettingItemVM _selectedColumn;

        [ObservableProperty]
        private int _frozenColumnCount;

        public bool IsSaved { get; private set; }

        public ColumnSettingsVM(SproutDataGrid grid)
        {
            _grid = grid;
            _configurationService = SproutApp.Services?.GetService<IConfigurationService>();

            Load();
        }

        private void Load()
        {
            var ordered = _grid.dataGrid.Columns
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            foreach (var col in ordered)
            {
                Columns.Add(new ColumnSettingItemVM
                {
                    Key = _grid.GetColumnKey(col),
                    Header = col.Header?.ToString(),
                    IsVisible = col.Visibility == Visibility.Visible
                });
            }

            FrozenColumnCount = _grid.dataGrid.FrozenColumnCount;
        }

        [RelayCommand]
        private void MoveUp()
        {
            if (SelectedColumn == null) return;
            int index = Columns.IndexOf(SelectedColumn);
            if (index > 0)
            {
                Columns.Move(index, index - 1);
            }
        }

        [RelayCommand]
        private void MoveDown()
        {
            if (SelectedColumn == null) return;
            int index = Columns.IndexOf(SelectedColumn);
            if (index >= 0 && index < Columns.Count - 1)
            {
                Columns.Move(index, index + 1);
            }
        }

        [RelayCommand]
        private void Save()
        {
            var layout = new SproutGridColumnLayout
            {
                FrozenColumnCount = FrozenColumnCount,
                Columns = Columns
                    .Select(c => new SproutGridColumnState
                    {
                        Key = c.Key,
                        IsVisible = c.IsVisible
                    })
                    .ToList()
            };

            _grid.ApplyColumnLayout(layout);

            var gridName = _grid.Config?.Name;
            if (_configurationService != null && !string.IsNullOrEmpty(gridName))
            {
                var config = _configurationService.Load();
                config.Settings.GridColumnLayouts[gridName] = layout;
                _configurationService.Save(config);
            }

            IsSaved = true;
        }
    }

    public partial class ColumnSettingItemVM : ObservableObject
    {
        public string Key { get; set; }

        [ObservableProperty]
        private string _header;

        [ObservableProperty]
        private bool _isVisible = true;
    }
}
