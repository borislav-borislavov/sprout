using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.UIStates
{
    public partial class SproutGridUIState : BaseUIState
    {
        [ObservableProperty]
        private object _selected;

        public virtual void SetUpState(SproutDataGrid control)
        {
            // Bindings and other setup logic can be added here if needed

            control.UIState = this;
            this.Name = control.Name;

            control.dataGrid.SetBinding(DataGrid.SelectedItemProperty,
                new Binding(nameof(this.Selected))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                });
        }
    }
}
