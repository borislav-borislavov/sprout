using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
    }
}
