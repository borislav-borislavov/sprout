using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Features.DataGridPresetsFeature;
using Sprout.Core.Messages;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Views.Controls;
using Sprout.Core.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutDataGridVM : BaseSproutControlVM, BusyUIState, IButtonActionHost, IDataAdapterHost, IDataAdapterDictionaryHost
    {
        [ObservableProperty]
        private object _selected;

        [ObservableProperty]
        private bool _isBusy;

        /// <summary>
        /// The grid this state is associated with. Used to read/apply the column settings
        /// (visibility, order and frozen count) e.g. from the column settings dialog or when exporting.
        /// </summary>
        public SproutDataGrid Grid { get; set; }

        public string JsonData
        {
            get
            {
                if (Grid.dataGrid.ItemsSource is not System.Data.DataView dv)
                {
                    return string.Empty;
                }

                return dv.Table.ToJson();
            }
        }

        [ObservableProperty]
        private bool _hasChanges;

        public Dictionary<string, IButtonAction> ButtonActions { get; } = [];

        [ObservableProperty]
        private IDataAdapter _dataAdapter;
        private IConfigurationService _configurationService;
        private IDialogService _dialogService;

        public Dictionary<string, IDataAdapter> DataAdapters { get; set; } = [];

        /// <summary>
        /// Raised when the user changes the grid's column layout so it can be persisted by the page.
        /// </summary>
        public event EventHandler<SproutGridColumnLayout> ColumnLayoutChanged;

        public SproutDataGridVM(string name, IConfigurationService configurationService, IDialogService dialogService, Guid ownerPageID) : base(name, ownerPageID)
        {
            _configurationService = configurationService;
            _dialogService = dialogService;
        }

        public bool ConfirmRefresh()
        {
            return !HasChanges || _dialogService.ShowMessage(
                "Refreshing this grid will discard unsaved changes. Do you want to continue?",
                "Confirm Refresh",
                DialogButton.YesNo,
                DialogImage.Warning) == DialogResult.Yes;
        }

        public virtual void SetUpState(SproutDataGrid control)
        {
            control.dataGrid.SetBinding(DataGrid.SelectedItemProperty,
                new Binding(nameof(this.Selected))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                });

            control.dataGrid.SetBinding(DataGrid.IsReadOnlyProperty,
                new Binding(nameof(this.IsBusy))
                {
                    Source = this,
                    Mode = BindingMode.OneWay
                });

            //Needed to properly track HasChanges which is responsible for providing a visual cue to the user that the grid has unsaved changes.
            //This approach is extremely light and it doesn't require a full DataTable scan.
            if (DataAdapter?.DataProvider is ObservableObject observableDataProvider)
            {
                observableDataProvider.PropertyChanged += ObservableDataProvider_PropertyChanged;
            }
        }

        #region Filter presets
        private IGridFilterPresetService _filterPresetService;
        private bool _isLoadingFilterPresets;

        public ObservableCollection<FilterPresetItem> FilterPresets { get; } = [];

        private FilterPresetItem _selectedFilterPreset;

        public FilterPresetItem SelectedFilterPreset
        {
            get => _selectedFilterPreset;
            set
            {
                if (!SetProperty(ref _selectedFilterPreset, value) || _isLoadingFilterPresets || value == null)
                    return;

                if (value.IsNone)
                {
                    ClearFilterPreset();
                }
                else
                {
                    ApplyFilterPreset(value.Name);
                }
            }
        }

        /// <summary>
        /// Hooks the preset service into this VM and applies the default preset. OwnerPageID is
        /// assigned in the constructor, before the initial data load - so the preset values
        /// participate in the first query exactly like the filters' configured DefaultValue.
        /// </summary>
        public void InitializeFilterPresets(IGridFilterPresetService filterPresetService)
        {
            _filterPresetService = filterPresetService;

            LoadFilterPresets();
        }

        public void LoadFilterPresets(string presetToSelect = null)
        {
            var collection = _filterPresetService.GetCollection(OwnerPageID, Name);

            presetToSelect ??= SelectedFilterPreset is { IsNone: false }
                ? SelectedFilterPreset.Name
                : collection.SelectedPresetName;

            _isLoadingFilterPresets = true;

            try
            {
                FilterPresets.Clear();
                FilterPresets.Add(new FilterPresetItem("None", isNone: true));

                foreach (var presetName in collection.Presets.Keys.OrderBy(name => name))
                {
                    FilterPresets.Add(new FilterPresetItem(presetName)
                    {
                        IsDefault = presetName == collection.SelectedPresetName
                    });
                }

                SelectedFilterPreset = FilterPresets.FirstOrDefault(item => item.Name == presetToSelect)
                    ?? FilterPresets[0];
            }
            finally
            {
                _isLoadingFilterPresets = false;
            }

            if (SelectedFilterPreset is { IsNone: false })
            {
                ApplyFilterPreset(SelectedFilterPreset.Name);
            }
        }

        /// <summary>
        /// Applies the named preset's filter values and, when captured, its column layout.
        /// </summary>
        public void ApplyFilterPreset(string presetName)
        {
            var collection = _filterPresetService.GetCollection(OwnerPageID, Name);

            if (!collection.Presets.TryGetValue(presetName, out var preset))
                return;

            var filters = DataAdapter.DataProvider.Filters;

            foreach (var (filterTitle, state) in preset.Filters)
            {
                if (!filters.TryGetValue(filterTitle, out var filter))
                    continue;

                filter.StartValue = state.Start;

                if (filter.IsRange)
                {
                    filter.EndValue = state.End;
                }
            }

            if (preset.ColumnLayout != null)
            {
                ApplyColumnLayout(preset.ColumnLayout);
            }

        }

        /// <summary>
        /// Clears all filter values without changing the configured default preset.
        /// </summary>
        public void ClearFilterPreset()
        {
            foreach (var filter in DataAdapter.DataProvider.Filters.Values)
            {
                filter.StartValue = null;
                filter.EndValue = null;
            }
        }

        /// <summary>
        /// Saves the current filter values and column layout under the given preset name.
        /// </summary>
        public void SaveFilterPreset(string presetName)
        {
            var preset = new GridFilterPreset
            {
                Filters = DataAdapter.DataProvider.Filters.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new FilterValueState
                    {
                        Start = kvp.Value.StartValue?.ToString(),
                        End = kvp.Value.EndValue?.ToString()
                    }),
                ColumnLayout = Grid?.GetCurrentLayout()
            };

            _filterPresetService.SavePreset(OwnerPageID, Name, presetName, preset);
            LoadFilterPresets(presetName);
        }

        /// <summary>
        /// Prompts for a preset name and saves the current filter values under it.
        /// </summary>
        [RelayCommand]
        private void PromptSaveFilterPreset()
        {
            var dialog = new TextInputWindow(
                "Save Filter Preset",
                "Preset name:",
                SelectedFilterPreset is { IsNone: false } selectedPreset
                    ? selectedPreset.Name
                    : string.Empty)
            {
                Owner = Window.GetWindow(Grid)
            };

            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.Value))
                return;

            SaveFilterPreset(dialog.Value.Trim());
        }

        [RelayCommand]
        private void SetSelectedFilterPresetAsDefault()
        {
            if (SelectedFilterPreset is not { IsNone: false } selectedPreset)
                return;

            _filterPresetService.SetDefaultPreset(OwnerPageID, Name, selectedPreset.Name);

            foreach (var preset in FilterPresets)
            {
                preset.IsDefault = preset.Name == selectedPreset.Name;
            }
        }

        [RelayCommand]
        private void DeleteSelectedFilterPreset()
        {
            if (SelectedFilterPreset is not { IsNone: false } selectedPreset)
                return;

            var result = _dialogService.ShowMessage($"Are you sure you want to delete {selectedPreset.Name}", DialogButton.YesNo);

            if (result != DialogResult.Yes) return;

            try
            {
                _filterPresetService.DeletePreset(OwnerPageID, Name, selectedPreset.Name);
                LoadFilterPresets();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// True when a default preset is configured and it carries a column layout.
        /// Used to skip the globally persisted column layout, which would otherwise
        /// fight with the layout restored by the preset.
        /// </summary>
        private bool SelectedFilterPresetHasColumnLayout()
        {
            if (_filterPresetService == null)
                return false;

            var collection = _filterPresetService.GetCollection(OwnerPageID, Name);

            return collection.SelectedPresetName != null
                && collection.Presets.TryGetValue(collection.SelectedPresetName, out var preset)
                && preset.ColumnLayout != null;
        }
        #endregion

        private void ObservableDataProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IDataProvider.Data)) return;

            //This runs before the ItemsSource binding pushes the new table into the grid,
            //so the current scroll position and selection can still be captured and later restored.
            if (Grid?.Config?.PreserveStateOnRefresh == true)
            {
                Grid.StatePreserver.Capture();
            }

            HasChanges = false;
            //Upon refreshing the data, the DataTable will be replaced with a new instance.
            //This means that the DataTable's RowChanged event will no longer be subscribed to. We need to re-subscribe to it.
            DataAdapter.DataProvider.Data?.DefaultView.ListChanged += DefaultView_ListChanged;
        }

        private void DefaultView_ListChanged(object? sender, ListChangedEventArgs e)
        {
            try
            {
                //When doing local search NewIndex is -1, I chose to keep the old state here
                if (e.NewIndex == -1) return;

                var dataRow = DataAdapter.DataProvider.Data?.Rows[e.NewIndex];
                if (dataRow == null)
                {
                    HasChanges = false;
                    return;
                }

                if (dataRow.RowState == System.Data.DataRowState.Added)
                {
                    HasChanges = true;
                    return;
                }

                if (dataRow.RowState != System.Data.DataRowState.Unchanged)
                {
                    // If it's Modified, we must diff the columns
                    foreach (System.Data.DataColumn col in dataRow.Table.Columns)
                    {
                        object originalVal = dataRow[col, System.Data.DataRowVersion.Original];
                        object currentVal = dataRow[col, System.Data.DataRowVersion.Current];

                        if (!Equals(originalVal, currentVal))
                        {
                            HasChanges = true;
                            return;
                        }
                    }
                }

                HasChanges = false;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error while checking for changes in the data grid. {ex.Message}");
            }
        }

        /// <summary>
        /// Opens one of the configured "Row" pages for the currently selected row.
        /// </summary>
        [RelayCommand]
        private void OpenRowActionPage(SproutDataGridRowActionConfig rowAction)
        {
            if (rowAction == null) return;

            var row = Selected;

            //No selection, but the grid has exactly one row: act on that row.
            //Exclude WPF's "new row" placeholder present on editable grids.
            if (row == null && Grid != null)
            {
                var rows = Grid.dataGrid.Items.Cast<object>()
                    .Where(i => i != CollectionView.NewItemPlaceholder)
                    .Take(2)
                    .ToList();

                if (rows.Count == 1)
                {
                    row = rows[0];
                }
            }

            if (row == null) return;

            var args = new OpenTabMessageArgs()
            {
                PageConfigID = rowAction.PageID,
                Parameter = row,
                OpenAsDialog = rowAction.OpenAsDialog,
                ParentPageID = OwnerPageID,
                OpenParentPageOnClose = rowAction.OpenParentPageOnClose
            };

            //Close the current page before opening the new one. Opening as a dialog
            //blocks until the dialog is dismissed, so closing afterwards would leave
            //the current page visible for the dialog's whole lifetime.
            if (rowAction.CloseCurrentPage)
            {
                WeakReferenceMessenger.Default.Send(new CloseTabMessage(new CloseTabMessageArgs
                {
                    PageConfigID = OwnerPageID
                }));
            }

            WeakReferenceMessenger.Default.Send(new OpenTabMessage(args));
        }

        [RelayCommand]
        private void DisplayItemPage(object parameter)
        {
            if (Selected == null) return;

            var args = new OpenTabMessageArgs()
            {
                PageConfigID = Grid.Config.ItemDisplayPage,
                Parameter = this.Selected
            };

            WeakReferenceMessenger.Default.Send(new OpenTabMessage(args));
        }

        #region ColumnLayout
        /// <summary>
        /// Applies a column layout to the grid without raising <see cref="ColumnLayoutChanged"/>.
        /// Used when restoring a persisted layout.
        /// </summary>
        public void ApplyColumnLayout(SproutGridColumnLayout layout)
            => Grid?.ApplyColumnLayout(layout);

        /// <summary>
        /// Applies a user-selected column layout to the grid and notifies listeners
        /// (e.g. the page) so the change can be persisted.
        /// </summary>
        public void UpdateColumnLayout(SproutGridColumnLayout layout)
        {
            Grid?.ApplyColumnLayout(layout);
            ColumnLayoutChanged?.Invoke(this, layout);
        }

        /// <summary>
        /// Restores any persisted column layout for the given grid and keeps it in sync with
        /// the configuration when the user changes it. When filter presets are enabled, the
        /// restore is skipped when the default preset carries its own column layout - applying
        /// the preset would overwrite it anyway.
        /// </summary>
        public void RegisterGridColumnLayout()
        {
            if (Grid?.Config?.Name is not string gridName || string.IsNullOrEmpty(gridName))
                return;

            var settings = _configurationService.Load().Settings;

            if (settings.GridColumnLayouts.TryGetValue(gridName, out var layout))
            {
                //Runs after InitializeFilterPresets, so the default preset (and its layout)
                //is already applied at this point.
                if (!SelectedFilterPresetHasColumnLayout())
                {
                    ApplyColumnLayout(layout);
                }
            }

            ColumnLayoutChanged += (_, updatedLayout) =>
            {
                try
                {
                    var config = _configurationService.Load();
                    config.Settings.GridColumnLayouts[gridName] = updatedLayout;
                    _configurationService.Save(config);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage(ex.Message, "Column layout Error", DialogButton.OK, DialogImage.Error);
                }
            };
        } 
        #endregion
    }
}
