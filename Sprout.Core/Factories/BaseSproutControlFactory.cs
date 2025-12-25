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
    public class BaseSproutControlFactory
    {
        protected static void AddControl<T>(T control, Dictionary<string, UIElement> controls)
            where T : FrameworkElement
        {
            if (string.IsNullOrWhiteSpace(control.Name))
            {
                var index = 1;
                var nameCandidate = $"{control.GetType().Name}{index}";

                while (controls.ContainsKey(nameCandidate))
                {
                    index++;
                    nameCandidate = $"{control.GetType().Name}{index}";
                }

                control.Name = nameCandidate;
            }

            controls[control.Name] = control;
        }

        protected static void SetGridPosition(FrameworkElement control, SproutControlConfig config)
        {
            Grid.SetRow(control, config.Row);
            Grid.SetColumn(control, config.Column);
            Grid.SetRowSpan(control, config.RowSpan);
            Grid.SetColumnSpan(control, config.ColumnSpan);
        }
    }
}
