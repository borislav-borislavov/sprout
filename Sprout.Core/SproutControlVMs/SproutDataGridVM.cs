using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Messages;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Views.Controls;
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
        public SproutDataGrid Grid { get; private set; }

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

        public Dictionary<string, IButtonAction> ButtonActions { get; } = [];
        //public IDataAdapter DataAdapter { get; set; }

        [ObservableProperty]
        private IDataAdapter _dataAdapter;
        private IConfigurationService _configurationService;
        private IDialogService _dialogService;

        public Dictionary<string, IDataAdapter> DataAdapters { get; set; } = [];

        /// <summary>
        /// Raised when the user changes the grid's column layout so it can be persisted by the page.
        /// </summary>
        public event EventHandler<SproutGridColumnLayout> ColumnLayoutChanged;

        public SproutDataGridVM(string name, IConfigurationService configurationService, IDialogService dialogService) : base(name)
        {
            _configurationService = configurationService;
            _dialogService = dialogService;
        }

        public virtual void SetUpState(SproutDataGrid control)
        {
            // Bindings and other setup logic can be added here if needed

            this.Grid = control;

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
        }

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

        /// <summary>
        /// Opens one of the configured "Row" pages for the currently selected row.
        /// </summary>
        [RelayCommand]
        private void OpenRowActionPage(SproutDataGridRowActionConfig rowAction)
        {
            if (Selected == null || rowAction == null) return;

            var args = new OpenTabMessageArgs()
            {
                PageConfigID = rowAction.PageID,
                Parameter = this.Selected,
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

        /// <summary>
        /// Restores any persisted column layout for the given grid and keeps it in sync with
        /// the configuration when the user changes it.
        /// </summary>
        public void RegisterGridColumnLayout()
        {
            if (Grid?.Config?.Name is not string gridName || string.IsNullOrEmpty(gridName))
                return;

            var settings = _configurationService.Load().Settings;

            if (settings.GridColumnLayouts.TryGetValue(gridName, out var layout))
            {
                ApplyColumnLayout(layout);
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
    }
}
