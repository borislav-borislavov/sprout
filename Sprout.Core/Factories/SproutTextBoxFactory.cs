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
    public class SproutTextBoxFactory : BaseSproutControlFactory
    {
        public static SproutTextBox Create(SproutTextBoxConfig config,
            Dictionary<string, UIElement> controls)
        {
            var sproutTextBox = new SproutTextBox
            {
                Name = config.Name,
                Config = config,
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

            AddControl(sproutTextBox, controls);

            SetGridPosition(sproutTextBox, config);

            var uiState = new SproutTextBoxUIState();
            uiState.SetUpState(sproutTextBox);

            return sproutTextBox;
        }
    }
}
