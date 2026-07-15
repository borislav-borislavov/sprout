using Sprout.Core.Models;
using Sprout.Core.Models.ButtonActions;
using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using Sprout.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.Features.ButtonActions
{
    public static class ButtonActionExtensions
    {
        internal static void BindButtonAction<C, V>(this ISproutControl<C, V> control, FrameworkElement bindable, IButtonAction action)
            where C : SproutControlConfig where V : BaseSproutControlVM
        {
            var buttonActionClassName = action.GetType().Name;

            if (control.VM is not IButtonActionHost buttonActionHost)
                return;

            if (buttonActionHost.ButtonActions.ContainsKey(buttonActionClassName) == false)
            {
                //add action to collection
                buttonActionHost.ButtonActions[buttonActionClassName] = action;
            }

            //set command binding 
            bindable.SetBinding(Button.CommandProperty,
                new Binding(nameof(SproutPageVM.PerformActionCommand))
                {
                    Mode = BindingMode.OneWay
                });

            //set command parameter binding
            bindable.SetBinding(Button.CommandParameterProperty, new Binding()
            {
                Mode = BindingMode.OneWay,
                Source = buttonActionHost,
                Path = new PropertyPath($"ButtonActions[{buttonActionClassName}]")
            });
        }
    }
}
