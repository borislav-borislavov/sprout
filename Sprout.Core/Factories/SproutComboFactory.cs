using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Factories
{
    public class SproutComboFactory : BaseSproutControlFactory
    {
        public static SproutCombo Create(SproutComboConfig sproutComboConfig,
            Dictionary<string, UIElement> controls)
        {
            var sproutCombo = new SproutCombo
            {
                Name = sproutComboConfig.Name,
                Config = sproutComboConfig,
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

            AddControl(sproutCombo, controls);

            SetPositionInGrid(sproutCombo, sproutComboConfig);

            var uiState = new SproutComboUIState();
            uiState.SetUpState(sproutCombo);

            return sproutCombo;
        }
    }
}
