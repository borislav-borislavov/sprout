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
using System.Windows.Input;

namespace Sprout.Core.SproutControlVMs
{
    public partial class SproutTextBoxUIState : BaseSproutControlVM, IDependent
    {
        [ObservableProperty]
        private string _text;

        public IEnumerable<DataProviderDependency> Dependencies { get; set; } = new List<DataProviderDependency>();

        public SproutTextBoxUIState(string name) : base(name)
        {
            
        }

        public virtual void SetUpState(SproutTextBox control)
        {
            control.VM = this;
            this.Name = control.Name;

            if (control.Config.ChangeValueOnEnter)
            {
                control.textBox.SetBinding(TextBox.TextProperty,
                    new Binding(nameof(this.Text))
                    {
                        Source = this,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                    });

                // 1. Create a custom command for the Enter key
                RoutedCommand triggerUpdateCommand = new RoutedCommand();

                // 2. Define what happens when that command runs
                CommandBinding commandBinding = new CommandBinding(triggerUpdateCommand, (sender, e) =>
                {
                    // Grab the text binding and force it to update your property
                    BindingExpression binding = control.textBox.GetBindingExpression(TextBox.TextProperty);
                    binding?.UpdateSource();
                });

                // 3. Generate the InputBinding (KeyBinding) for the Enter key
                KeyBinding enterBinding = new KeyBinding(triggerUpdateCommand, Key.Return, ModifierKeys.None);

                // 4. Register everything onto the TextBox
                control.textBox.CommandBindings.Add(commandBinding);
                control.textBox.InputBindings.Add(enterBinding);
            }
            else
            {
                control.textBox.SetBinding(TextBox.TextProperty,
                    new Binding(nameof(this.Text))
                    {
                        Source = this,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
            }
        }

        public void DepenencyChanged(DataProviderDependency changedDependency, VMRegistry vmRegistry)
        {
            var targetedControlUIState = vmRegistry[changedDependency.ControlName];

            if (targetedControlUIState == null)
                throw new Exception($"UI State for control {changedDependency.ControlName} not found.");

            this.Text = $"{BindingEvaluator.Evaluate(targetedControlUIState, changedDependency.PropertyPath)}";
        }
    }
}
