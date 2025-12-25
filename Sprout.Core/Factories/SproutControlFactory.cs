using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
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
        public static UIElement GetControl(SproutControlConfig sControl, 
            Dictionary<string, UIElement> controls)
        {
            switch (sControl)
            {
                case GridConfig gridConfig:
                    return GridFactory.GenerateGrid(gridConfig, controls);
                case ButtonConfig buttonConfig:
                    return ButtonFactory.GenerateButton(buttonConfig, controls);
                case SproutDataGridConfig sproutGridConfig:
                    return SproutDataGridFactory.GenerateSproutGrid(sproutGridConfig, controls);
                case SproutComboConfig sproutComboConfig:
                    return SproutComboFactory.Create(sproutComboConfig, controls);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
