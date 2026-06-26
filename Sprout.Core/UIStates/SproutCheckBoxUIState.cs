using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class SproutCheckBoxUIState : BaseUIState
    {
        [ObservableProperty]
        private bool _isChecked;

        public virtual void SetUpState(SproutCheckBox control)
        {
            control.UIState = this;
            this.Name = control.Name;

            control.checkBox.SetBinding(CheckBox.IsCheckedProperty,
                new Binding(nameof(this.IsChecked))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        }
    }
}
