using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Factories
{
    public class SproutComboFactory : BaseSproutControlFactory
    {
        public static SproutCombo Create(SproutComboConfig sproutComboConfig,
            Dictionary<string, UIElement> controls)
        {
            var sproutCombo = new SproutCombo
            {
                Name = sproutComboConfig.Name,
                Config = sproutComboConfig,
            };

            sproutCombo.comboBox.DisplayMemberPath = sproutComboConfig.DisplayColumn;
            sproutCombo.comboBox.SelectedValuePath = sproutComboConfig.ValueColumn;

            if (!string.IsNullOrEmpty(sproutComboConfig.VerticalAlignment))
            {
                if (Enum.TryParse<VerticalAlignment>(sproutComboConfig.VerticalAlignment, true, out var alignment))
                {
                    sproutCombo.comboBox.VerticalAlignment = alignment;
                }
                else
                {
#warning show a warning that the alignment is invalid
                }
            }



            AddControl(sproutCombo, controls);

            SetPositionInGrid(sproutCombo, sproutComboConfig);

            var uiState = new SproutComboUIState();
            uiState.SetUpState(sproutCombo);

            return sproutCombo;
        }
    }
}
