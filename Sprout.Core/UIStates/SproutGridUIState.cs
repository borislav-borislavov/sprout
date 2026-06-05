using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Views.Controls;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.UIStates
{
    public partial class SproutGridUIState : BaseUIState, BusyUIState
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
                if (Grid.dataGrid.ItemsSource is not DataView dv)
                {
                    return string.Empty;
                }

                return dv.Table.ToJson();
            }
        }

        /// <summary>
        /// Raised when the user changes the grid's column layout so it can be persisted by the page.
        /// </summary>
        public event EventHandler<SproutGridColumnLayout> ColumnLayoutChanged;

        public virtual void SetUpState(SproutDataGrid control)
        {
            // Bindings and other setup logic can be added here if needed

            control.UIState = this;
            this.Name = control.Name;
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
    }
}
