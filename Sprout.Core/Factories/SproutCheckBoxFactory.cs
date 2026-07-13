using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
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
    public class SproutCheckBoxFactory : BaseSproutControlFactory, ISproutCheckBoxFactory
    {
        public SproutCheckBox Create(SproutCheckBoxConfig config)
        {
            var sproutCheckBox = new SproutCheckBox
            {
                Name = config.Name,
                Config = config,
            };

            if (config.Height.HasValue)
            {
                sproutCheckBox.checkBox.Height = config.Height.Value;
            }

            if (config.Width.HasValue)
            {
                sproutCheckBox.checkBox.Width = config.Width.Value;
            }

            if (!string.IsNullOrEmpty(config.Title))
            {
                sproutCheckBox.checkBox.Content = config.Title;
            }

            if (!string.IsNullOrWhiteSpace(config.Margin))
            {
                if (new ThicknessConverter().ConvertFromString(config.Margin) is Thickness margin)
                {
                    sproutCheckBox.Margin = margin;
                }
            }

            if (!string.IsNullOrEmpty(config.HorizontalAlignment) &&
                config.HorizontalAlignment != "(Default)" &&
                Enum.TryParse<HorizontalAlignment>(config.HorizontalAlignment, out var hAlign))
            {
                sproutCheckBox.HorizontalAlignment = hAlign;
            }

            if (!string.IsNullOrEmpty(config.VerticalAlignment) &&
                config.VerticalAlignment != "(Default)" &&
                Enum.TryParse<VerticalAlignment>(config.VerticalAlignment, out var vAlign))
            {
                sproutCheckBox.VerticalAlignment = vAlign;
            }

            if (!string.IsNullOrEmpty(config.ToolTip))
            {
                sproutCheckBox.ToolTip = config.ToolTip;
            }

            SetPositionInGrid(sproutCheckBox, config);

            var uiState = new SproutCheckBoxUIState(sproutCheckBox.Name);
            uiState.SetUpState(sproutCheckBox);

            return sproutCheckBox;
        }
    }
}
