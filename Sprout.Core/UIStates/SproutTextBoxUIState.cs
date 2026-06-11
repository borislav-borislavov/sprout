using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
    }
}
