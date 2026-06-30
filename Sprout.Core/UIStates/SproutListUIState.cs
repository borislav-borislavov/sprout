using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Views.Controls;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.UIStates
{
    public partial class SproutListUIState : BaseUIState
    {
        /// <summary>
        /// The current search/filter text of the list. Exposed on the UI state so other controls and
        /// data providers can react to it through the dependency system.
        /// </summary>
        [ObservableProperty]
        private string _searchText;

        /// <summary>
        /// The item currently selected in the list. Used by the page-launch menu to pass the selected
        /// row to the opened page.
        /// </summary>
        [ObservableProperty]
        private object _selected;

        public virtual void SetUpState(SproutList control)
        {
            control.UIState = this;
            this.Name = control.Name;

            control.SetBinding(SproutList.SearchTextProperty,
                new Binding(nameof(this.SearchText))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                });

            control.itemsControl.SetBinding(ListBox.SelectedItemProperty,
                new Binding(nameof(this.Selected))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                });
        }
    }
}
