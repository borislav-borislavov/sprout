using Sprout.Core.Models.Configurations;
using System.Windows;

namespace Sprout.Core.Factories
{
    public interface ISproutControlFactory
    {
        UIElement GetControl(SproutControlConfig sControl, Dictionary<string, UIElement> controls);
    }
}