using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Queries;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sprout.Core.Factories
{
    public class SproutComboFactory : BaseSproutControlFactory, ISproutComboFactory
    {
        public SproutCombo Create(SproutComboConfig sproutComboConfig)
        {
            var sproutCombo = new SproutCombo
            {
                Name = sproutComboConfig.Name,
                Config = sproutComboConfig,
                VM = new SproutComboVM(sproutComboConfig.Name)
            };

            sproutCombo.comboBox.DisplayMemberPath = sproutComboConfig.DisplayColumn;
            sproutCombo.comboBox.SelectedValuePath = sproutComboConfig.ValueColumn;

            if (sproutComboConfig.Height.HasValue)
                sproutCombo.comboBox.Height = sproutComboConfig.Height.Value;

            if (sproutComboConfig.Width.HasValue)
                sproutCombo.comboBox.Width = sproutComboConfig.Width.Value;

            if (!string.IsNullOrWhiteSpace(sproutComboConfig.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(sproutComboConfig.Margin) is Thickness margin)
                    sproutCombo.Margin = margin;
            }

            if (!string.IsNullOrEmpty(sproutComboConfig.HorizontalAlignment) &&
                sproutComboConfig.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(sproutComboConfig.HorizontalAlignment, out var hAlign))
            {
                sproutCombo.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(sproutComboConfig.VerticalAlignment) &&
                sproutComboConfig.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(sproutComboConfig.VerticalAlignment, true, out var vAlign))
            {
                sproutCombo.comboBox.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(sproutComboConfig.ToolTip))
                sproutCombo.ToolTip = sproutComboConfig.ToolTip;

            SetPositionInGrid(sproutCombo, sproutComboConfig);

            SetupVM(sproutCombo);

            return sproutCombo;
        }

        private void SetupVM(SproutCombo sproutCombo)
        {
            sproutCombo.VM.SetUpState(sproutCombo);

            sproutCombo.comboBox.SetBinding(ComboBox.ItemsSourceProperty,
                new Binding()
                {
                    Mode = BindingMode.OneWay,
                    Source = sproutCombo.VM,
                    Path = new PropertyPath("DataAdapter.DataProvider.Data")
                });

            if (!string.IsNullOrEmpty(sproutCombo.Config.SelectedValue))
            {
                var dependency = DependencyParser.ParseDependencies(sproutCombo.Config.SelectedValue).FirstOrDefault();

                if (dependency != null)
                {
                    sproutCombo.comboBox.SetBinding(
                        ComboBox.SelectedValueProperty,
                        new Binding
                        {
#warning this will not work anymore
                            //Source = vm.VMRegistry,
                            Path = new PropertyPath($"[{dependency.ControlName}].{dependency.PropertyPath}"),
                            Mode = BindingMode.TwoWay
                        });
                }
                else if (int.TryParse(sproutCombo.Config.SelectedValue, out var selIdx))
                {
                    sproutCombo.comboBox.SelectedIndex = selIdx;
                }
                else
                {
                    sproutCombo.comboBox.SelectedValue = sproutCombo.Config.SelectedValue;
                }
            }
        }
    }
}
