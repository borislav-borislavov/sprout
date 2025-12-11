using Sprout.Core.Models.Configurations;
using Sprout.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Factories
{
    public static class SproutControlFactory
    {
        public static UIElement GetControl(SproutConfig sControl, 
            Dictionary<string, UIElement> controls)
        {
            switch (sControl)
            {
                case GridConfig gridConfig:
                    return GridFactory.GenerateGrid(gridConfig, controls);
                case ButtonConfig buttonConfig:
                    return ButtonFactory.GenerateButton(buttonConfig, controls);
                default:
                    return null;
            }
        }
    }
}
