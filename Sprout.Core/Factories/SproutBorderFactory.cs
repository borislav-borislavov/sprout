using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sprout.Core.Factories
{
    public class SproutBorderFactory : BaseSproutControlFactory
    {
        public static SproutBorder Create(SproutBorderConfig config,
            Dictionary<string, UIElement> controls)
        {
            var sproutBorder = new SproutBorder
            {
                Name = config.Name,
                Config = config,
            };

            if (!string.IsNullOrWhiteSpace(config.Background))
            {
                if (ColorConverter.ConvertFromString(config.Background) is Color color)
                {
                    sproutBorder.border.Background = new SolidColorBrush(color);
                }
            }

            if (!string.IsNullOrWhiteSpace(config.BorderBrush))
            {
                if (ColorConverter.ConvertFromString(config.BorderBrush) is Color color)
                {
                    sproutBorder.border.BorderBrush = new SolidColorBrush(color);
                }
            }

            sproutBorder.border.BorderThickness = new Thickness(config.BorderThickness);

            sproutBorder.border.CornerRadius = new CornerRadius(config.CornerRadius);

            if (config.Height.HasValue)
                sproutBorder.Height = config.Height.Value;

            if (config.Width.HasValue)
                sproutBorder.Width = config.Width.Value;

            if (!string.IsNullOrWhiteSpace(config.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(config.Margin) is Thickness margin)
                    sproutBorder.Margin = margin;
            }

            if (!string.IsNullOrWhiteSpace(config.Padding))
            {
                if (new ThicknessConverter().ConvertFromString(config.Padding) is Thickness padding)
                    sproutBorder.border.Padding = padding;
            }

            if (!string.IsNullOrEmpty(config.HorizontalAlignment) &&
                config.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(config.HorizontalAlignment, out var hAlign))
            {
                sproutBorder.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(config.VerticalAlignment) &&
                config.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(config.VerticalAlignment, out var vAlign))
            {
                sproutBorder.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(config.ToolTip))
                sproutBorder.ToolTip = config.ToolTip;

            // Render the single child control inside the border
            if (config.Child != null)
            {
                sproutBorder.border.Child = SproutControlFactory.GetControl(config.Child, controls) as UIElement;
            }

            AddControl(sproutBorder, controls);

            SetPositionInGrid(sproutBorder, config);

            var uiState = new SproutBorderUIState();
            uiState.SetUpState(sproutBorder.Name);
            sproutBorder.UIState = uiState;

            return sproutBorder;
        }
    }
}
