using Sprout.Core.Models.Configurations;
using Sprout.Core.Views.Controls;

namespace Sprout.Core.Factories
{
    public interface ISproutTabControlFactory
    {
        SproutTabControl Create(SproutTabControlConfig config);
    }
}
