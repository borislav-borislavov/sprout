using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Queries;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Sprout.Core.Factories
{
    public class SproutTextBoxFactory : BaseSproutControlFactory, ISproutTextBoxFactory
    {
        public SproutTextBox Create(SproutTextBoxConfig config, VMRegistry vmRegistry)
        {
            var sproutTextBox = new SproutTextBox
            {
                Name = config.Name,
                Config = config,
                VM = new SproutTextBoxVM(config.Name)
            };

            if (config.Height.HasValue)
            {
                sproutTextBox.textBox.Height = config.Height.Value;
            }

            if (config.Width.HasValue)
            {
                sproutTextBox.textBox.Width = config.Width.Value;
            }

            if (!string.IsNullOrEmpty(config.Title))
            {
                sproutTextBox.lblTitle.Text = config.Title;
                sproutTextBox.lblTitle.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(config.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(config.Margin) is Thickness margin)
                {
                    sproutTextBox.Margin = margin;
                }
            }

            if (!string.IsNullOrEmpty(config.Placeholder))
            {
                sproutTextBox.SetPlaceholder(config.Placeholder);
            }

            if (config.MultiLine)
            {
                sproutTextBox.textBox.TextWrapping = System.Windows.TextWrapping.Wrap;
                sproutTextBox.textBox.AcceptsReturn = true;
                sproutTextBox.textBox.VerticalAlignment = VerticalAlignment.Stretch;
                sproutTextBox.textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sproutTextBox.textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }

            if (!string.IsNullOrEmpty(config.HorizontalAlignment) &&
                config.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(config.HorizontalAlignment, out var hAlign))
            {
                sproutTextBox.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(config.VerticalAlignment) &&
                config.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(config.VerticalAlignment, out var vAlign))
            {
                sproutTextBox.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(config.ToolTip))
            {
                sproutTextBox.ToolTip = config.ToolTip;
            }

            if (config.AllowFileDrop)
            {
                sproutTextBox.EnableFileDrop();
            }

            SetPositionInGrid(sproutTextBox, config);

            sproutTextBox.VM.Dependencies = DependencyParser.ParseDependencies(sproutTextBox.Config.Binding);
            sproutTextBox.VM.SetUpState(sproutTextBox);

            if (config.TwoWayBinding && !string.IsNullOrEmpty(config.Binding))
            {
                var dependency = sproutTextBox.VM.Dependencies.FirstOrDefault();

                if (dependency != null)
                {
                    //The source VM may not be registered yet (control order), so the
                    //binding is attached on Loaded, after the whole page is constructed.
                    sproutTextBox.Loaded += (s, e) =>
                    {
                        var sourceVM = vmRegistry[dependency.ControlName];

                        if (BindingOperations.GetBinding(sproutTextBox.textBox, TextBox.TextProperty)?.Source == sourceVM)
                            return;

                        //Replaces the VM.Text binding from SetUpState. The binding engine keeps
                        //both directions in sync and suppresses feedback loops natively.
                        sproutTextBox.textBox.SetBinding(TextBox.TextProperty,
                            new Binding(dependency.PropertyPath)
                            {
                                Source = sourceVM,
                                Mode = BindingMode.TwoWay,
                                UpdateSourceTrigger = config.ChangeValueOnEnter
                                    ? UpdateSourceTrigger.Explicit
                                    : UpdateSourceTrigger.PropertyChanged
                            });
                    };

                    //Keep VM.Text in sync so page logic reading the VM still sees the value.
                    sproutTextBox.textBox.TextChanged += (s, e) =>
                        sproutTextBox.VM.Text = sproutTextBox.textBox.Text;
                }
            }

            return sproutTextBox;
        }
    }
}
