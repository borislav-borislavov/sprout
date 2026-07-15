using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class SproutLabelVM : BaseSproutControlVM
    {
        [ObservableProperty]
        private string _text;

        public SproutLabelVM(string name) : base(name)
        {

        }

        public virtual void SetUpState(SproutLabel control)
        {
            control.VM = this;

            control.textBlock.SetBinding(TextBlock.TextProperty,
                new Binding(nameof(this.Text))
                {
                    Source = this,
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        }
    }
}
