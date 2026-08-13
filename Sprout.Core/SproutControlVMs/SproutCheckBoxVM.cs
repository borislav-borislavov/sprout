using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Features.Dependency;
using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters.DataProviders;
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
    public partial class SproutCheckBoxVM : BaseSproutControlVM, IDependent
    {
        [ObservableProperty]
        private bool _isChecked;

        public IEnumerable<DataProviderDependency> Dependencies { get; set; } = new List<DataProviderDependency>();

        public SproutCheckBoxVM(string name) : base(name)
        {
            
        }

        public virtual void SetUpState(SproutCheckBox control)
        {
            control.VM = this;

            control.checkBox.SetBinding(CheckBox.IsCheckedProperty,
                new Binding(nameof(this.IsChecked))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        }

        public void DepenencyChanged(DataProviderDependency changedDependency, VMRegistry vmRegistry)
        {
            var targetedControlUIState = vmRegistry[changedDependency.ControlName];

            if (targetedControlUIState == null)
                throw new Exception($"VM for control {changedDependency.ControlName} not found.");

            var value = BindingEvaluator.Evaluate(targetedControlUIState, changedDependency.PropertyPath);

            if (value is bool boolValue)
            {
                this.IsChecked = boolValue;
            }
            else
            {
                this.IsChecked = Convert.ToBoolean(value);
            }
        }
    }
}
