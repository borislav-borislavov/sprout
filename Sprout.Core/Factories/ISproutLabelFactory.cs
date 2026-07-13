using Sprout.Core.Models.Configurations;
using Sprout.Core.Views.Controls;

namespace Sprout.Core.Factories
{
    public interface ISproutLabelFactory
    {
        SproutLabel Create(SproutLabelConfig config);
    }
}
