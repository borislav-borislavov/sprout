using Sprout.Core.Models.Configurations;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;

namespace Sprout.Core.Factories
{
    public interface ISproutTextBoxFactory
    {
        SproutTextBox Create(SproutTextBoxConfig config, VMRegistry vmRegistry);
    }
}
