using Sprout.Core.Models.Configurations;
using Sprout.Core.Views.Controls;

namespace Sprout.Core.Factories
{
    public interface ISproutButtonFactory
    {
        SproutButton Create(SproutButtonConfig sproutButtonConfig);
    }
}