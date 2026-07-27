using Sprout.Core.Models.Configurations;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public class SproutTabControlFactory : BaseSproutControlFactory, ISproutTabControlFactory
    {
        public SproutTabControl Create(SproutTabControlConfig config)
        {
            var sproutTabControl = new SproutTabControl
            {
                Name = config.Name,
                Config = config,
            };

            if (config.Height.HasValue)
                sproutTabControl.Height = config.Height.Value;

            if (config.Width.HasValue)
                sproutTabControl.Width = config.Width.Value;

            if (!string.IsNullOrWhiteSpace(config.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(config.Margin) is Thickness margin)
                    sproutTabControl.Margin = margin;
            }

            if (!string.IsNullOrEmpty(config.HorizontalAlignment) &&
                config.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(config.HorizontalAlignment, out var hAlign))
            {
                sproutTabControl.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(config.VerticalAlignment) &&
                config.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(config.VerticalAlignment, out var vAlign))
            {
                sproutTabControl.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(config.ToolTip))
                sproutTabControl.ToolTip = config.ToolTip;

            SetPositionInGrid(sproutTabControl, config);

            var vm = new SproutTabControlVM(sproutTabControl.Name);
            sproutTabControl.VM = vm;

            return sproutTabControl;
        }
    }
}
