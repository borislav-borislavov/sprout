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
    public partial class SproutTextBoxUIState : BaseUIState
    {
        [ObservableProperty]
        private string _text;

        public virtual void SetUpState(SproutTextBox control)
        {
            control.UIState = this;
            this.Name = control.Name;

            control.textBox.SetBinding(TextBox.TextProperty,
                new Binding(nameof(this.Text))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        }
    }
}
