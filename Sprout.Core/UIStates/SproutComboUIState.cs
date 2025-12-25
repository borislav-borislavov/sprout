using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.UIStates
{
    public partial class SproutComboUIState : BaseUIState
    {
        [ObservableProperty]
        private object _selected;

        public virtual void SetUpState(SproutCombo control)
        {
            // Bindings and other setup logic can be added here if needed

            control.UIState = this;
            this.Name = control.Name;

            control.comboBox.SetBinding(DataGrid.SelectedItemProperty,
                new Binding(nameof(this.Selected))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                });
        }
    }
}
