using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.UIStates;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sprout.Core.ViewModels
{
    public partial class ColumnSettingsVM : ObservableObject
    {
        private readonly SproutGridUIState _gridState;

        [ObservableProperty]
        private ObservableCollection<ColumnSettingItemVM> _columns = [];

        [ObservableProperty]
        private ColumnSettingItemVM _selectedColumn;

        [ObservableProperty]
        private int _frozenColumnCount;

        public bool IsSaved { get; private set; }

        public ColumnSettingsVM(SproutGridUIState gridState)
        {
            _gridState = gridState;

            Load();
        }

        private void Load()
        {
            var layout = _gridState.Grid.GetCurrentLayout();
            LoadLayout(layout);
        }

        private void LoadLayout(SproutGridColumnLayout layout)
        {
            Columns.Clear();

            foreach (var column in layout.Columns)
            {
                Columns.Add(new ColumnSettingItemVM
                {
                    Key = column.Key,
                    IsVisible = column.IsVisible
                });
            }

            FrozenColumnCount = layout.FrozenColumnCount;
        }

        [RelayCommand]
        private void ResetToDefault()
        {
            LoadLayout(_gridState.Grid.GetDefaultLayout());
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

            // The grid state applies the layout and notifies the page so it can be persisted.
            _gridState.UpdateColumnLayout(layout);

            IsSaved = true;
        }
    }

    public partial class ColumnSettingItemVM : ObservableObject
    {
        [ObservableProperty]
        private string _key;

        [ObservableProperty]
        private bool _isVisible = true;
    }
}
