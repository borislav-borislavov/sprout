using Sprout.Core.Models.Configurations;
using Sprout.Core.Views.Controls;

namespace Sprout.Core.Factories
{
    public interface IButtonFactory
    {
        static abstract SproutButton GenerateSproutButton(SproutButtonConfig sproutButtonConfig);
    }
}