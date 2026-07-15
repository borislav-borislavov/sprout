using Sprout.Core.Models.Configurations;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sprout.Core.Factories
{
    public class SproutLabelFactory : BaseSproutControlFactory, ISproutLabelFactory
    {
        public SproutLabel Create(SproutLabelConfig config)
        {
            var sproutLabel = new SproutLabel
            {
                Name = config.Name,
                Config = config,
            };

            if (!string.IsNullOrWhiteSpace(config.Foreground) &&
                ColorConverter.ConvertFromString(config.Foreground) is Color color)
            {
                sproutLabel.textBlock.Foreground = new SolidColorBrush(color);
            }

            if (!string.IsNullOrWhiteSpace(config.FontFamily))
            {
                sproutLabel.textBlock.FontFamily = new FontFamily(config.FontFamily);
            }

            if (config.FontSize.HasValue)
            {
                sproutLabel.textBlock.FontSize = config.FontSize.Value;
            }

            if (!string.IsNullOrWhiteSpace(config.FontWeight) &&
                new FontWeightConverter().ConvertFromString(config.FontWeight) is FontWeight fontWeight)
            {
                sproutLabel.textBlock.FontWeight = fontWeight;
            }

            if (!string.IsNullOrWhiteSpace(config.FontStyle) &&
                new FontStyleConverter().ConvertFromString(config.FontStyle) is FontStyle fontStyle)
            {
                sproutLabel.textBlock.FontStyle = fontStyle;
            }

            if (config.TextWrapping)
            {
                sproutLabel.textBlock.TextWrapping = TextWrapping.Wrap;
            }

            if (config.Height.HasValue)
            {
                sproutLabel.textBlock.Height = config.Height.Value;
            }

            if (config.Width.HasValue)
            {
                sproutLabel.textBlock.Width = config.Width.Value;
            }

            if (!string.IsNullOrWhiteSpace(config.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(config.Margin) is Thickness margin)
                {
                    sproutLabel.Margin = margin;
                }
            }

            if (!string.IsNullOrEmpty(config.HorizontalAlignment) &&
                config.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(config.HorizontalAlignment, out var hAlign))
            {
                sproutLabel.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(config.VerticalAlignment) &&
                config.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(config.VerticalAlignment, out var vAlign))
            {
                sproutLabel.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(config.ToolTip))
            {
                sproutLabel.ToolTip = config.ToolTip;
            }

            SetPositionInGrid(sproutLabel, config);

            var vm = new SproutLabelVM(sproutLabel.Name);
            vm.SetUpState(sproutLabel);
            vm.Text = config.Text;

            return sproutLabel;
        }
    }
}
