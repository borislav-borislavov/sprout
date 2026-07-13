using Sprout.Core.Models.Configurations;
using Sprout.Core.Views.Controls;
using System.Windows;

namespace Sprout.Core.Factories
{
    public interface ISproutListFactory
    {
        SproutList Create(SproutListConfig config, UIElement? itemTemplateRoot);
    }
}