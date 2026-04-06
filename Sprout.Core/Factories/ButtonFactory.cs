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

            AddControl(sproutButton, controls);

            SetPositionInGrid(sproutButton, sproutButtonConfig);

            return sproutButton;
        }
    }
}
