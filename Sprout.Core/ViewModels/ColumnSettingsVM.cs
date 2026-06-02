using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.UIStates;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Sprout.Core.ViewModels
{
    public partial class ColumnSettingsVM : ObservableObject
    {
        private readonly SproutGridUIState _gridState;

        [ObservableProperty]
        private ObservableCollection<ColumnSettingItemVM> _columns = [];

        [ObservableProperty]
        private ColumnSettingItemVM _selectedColumn;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    var previous = SelectedColumn;
                    FilteredColumns?.Refresh();
                    if (previous != null && FilteredColumns?.Contains(previous) == true)
                        SelectedColumn = previous;
                }
            }
        }

        [RelayCommand]
        private void ClearSearch() => SearchText = string.Empty;

        public ICollectionView FilteredColumns { get; private set; }

        [ObservableProperty]
        private int _frozenColumnCount;

        private bool? _allColumnsVisible = true;
        public bool? AllColumnsVisible
        {
            get => _allColumnsVisible;
            set
            {
                if (SetProperty(ref _allColumnsVisible, value) && value.HasValue)
                {
                    foreach (var column in Columns)
                        column.IsVisible = value.Value;
                }
            }
        }

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
            if (Columns != null)
                Columns.CollectionChanged -= OnColumnsCollectionChanged;

            Columns.Clear();

            foreach (var column in layout.Columns)
            {
                var item = new ColumnSettingItemVM
                {
                    Key = column.Key,
                    IsVisible = column.IsVisible
                };
                item.PropertyChanged += OnColumnItemPropertyChanged;
                Columns.Add(item);
            }

            Columns.CollectionChanged += OnColumnsCollectionChanged;
            FrozenColumnCount = layout.FrozenColumnCount;

            FilteredColumns = CollectionViewSource.GetDefaultView(Columns);
            FilteredColumns.Filter = o => o is ColumnSettingItemVM item &&
                (string.IsNullOrWhiteSpace(_searchText) ||
                 item.Key.Contains(_searchText, System.StringComparison.OrdinalIgnoreCase));
            OnPropertyChanged(nameof(FilteredColumns));

            RefreshAllColumnsVisible();
        }

        private void OnColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (ColumnSettingItemVM item in e.OldItems)
                    item.PropertyChanged -= OnColumnItemPropertyChanged;

            if (e.NewItems != null)
                foreach (ColumnSettingItemVM item in e.NewItems)
                    item.PropertyChanged += OnColumnItemPropertyChanged;

            RefreshAllColumnsVisible();
        }

        private void OnColumnItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColumnSettingItemVM.IsVisible))
                RefreshAllColumnsVisible();
        }

        private void RefreshAllColumnsVisible()
        {
            bool allChecked = Columns.All(c => c.IsVisible);
            bool allUnchecked = Columns.All(c => !c.IsVisible);
            _allColumnsVisible = allChecked ? true : allUnchecked ? false : null;
            OnPropertyChanged(nameof(AllColumnsVisible));
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
