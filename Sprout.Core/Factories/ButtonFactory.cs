using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sprout.Core.Factories
{
    public static class ButtonFactory
    {
        public static UIElement GenerateButton(ButtonConfig buttonConfig)
        {
            var button = new System.Windows.Controls.Button
            {
                Content = buttonConfig.Content ?? "Button"
            };

            Grid.SetRow(button, buttonConfig.Row);
            Grid.SetRowSpan(button, buttonConfig.RowSpan);

            Grid.SetColumn(button, buttonConfig.Column);
            Grid.SetColumnSpan(button, buttonConfig.ColumnSpan);

            return button;
        }
    }
}
