using Sprout.Core.Models.Configurations;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public class ButtonFactory : BaseSproutControlFactory
    {
        public static SproutButton GenerateSproutButton(SproutButtonConfig sproutButtonConfig, Dictionary<string, UIElement> controls)
        {
            var sproutButton = new SproutButton
            {
                Name = sproutButtonConfig.Name,
                Config = sproutButtonConfig,
            };

            sproutButton.button.Content = sproutButtonConfig.Content ?? "Button";

            if (sproutButtonConfig.Height.HasValue)
                sproutButton.button.Height = sproutButtonConfig.Height.Value;

            if (sproutButtonConfig.Width.HasValue)
                sproutButton.button.Width = sproutButtonConfig.Width.Value;

            if (!string.IsNullOrWhiteSpace(sproutButtonConfig.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(sproutButtonConfig.Margin) is Thickness margin)
                    sproutButton.Margin = margin;
            }

            if (!string.IsNullOrEmpty(sproutButtonConfig.HorizontalAlignment) &&
                sproutButtonConfig.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(sproutButtonConfig.HorizontalAlignment, out var hAlign))
            {
                sproutButton.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(sproutButtonConfig.VerticalAlignment) &&
                sproutButtonConfig.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(sproutButtonConfig.VerticalAlignment, out var vAlign))
            {
                sproutButton.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(sproutButtonConfig.ToolTip))
                sproutButton.ToolTip = sproutButtonConfig.ToolTip;

            if (!string.IsNullOrWhiteSpace(sproutButtonConfig.Padding))
            {
                if (new ThicknessConverter().ConvertFromString(sproutButtonConfig.Padding) is Thickness padding)
                    sproutButton.button.Padding = padding;
            }

            AddControl(sproutButton, controls);

            SetPositionInGrid(sproutButton, sproutButtonConfig);

            return sproutButton;
        }
    }
}
