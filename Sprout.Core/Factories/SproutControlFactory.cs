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
                case SproutBorderConfig sproutBorderConfig:
                    return SproutBorderFactory.Create(sproutBorderConfig, controls);
                case SproutButtonConfig sproutButtonConfig:
                    return ButtonFactory.GenerateSproutButton(sproutButtonConfig, controls);
                case SproutDataGridConfig sproutGridConfig:
                    return SproutDataGridFactory.GenerateSproutGrid(sproutGridConfig, controls);
                case SproutComboConfig sproutComboConfig:
                    return SproutComboFactory.Create(sproutComboConfig, controls);
                case SproutTextBoxConfig sproutTextBoxConfig:
                    return SproutTextBoxFactory.Create(sproutTextBoxConfig, controls);
                case SproutCheckBoxConfig sproutCheckBoxConfig:
                    return SproutCheckBoxFactory.Create(sproutCheckBoxConfig, controls);
                case SproutDatePickerConfig sproutDatePickerConfig:
                    return SproutDatePickerFactory.Create(sproutDatePickerConfig, controls);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
