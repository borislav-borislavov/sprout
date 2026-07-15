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

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutComboUIState : BaseSproutControlVM
    {
        [ObservableProperty]
        private object _selected;

        public SproutComboUIState(string name) : base(name)
        {
            
        }

        public virtual void SetUpState(SproutCombo control)
        {
            control.VM = this;

            control.comboBox.SetBinding(DataGrid.SelectedItemProperty,
                new Binding(nameof(this.Selected))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay
                });
        }
    }
}
