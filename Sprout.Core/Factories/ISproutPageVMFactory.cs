using Sprout.Core.Models.Configurations;
using Sprout.Core.ViewModels;

namespace Sprout.Core.Factories
{
    public interface ISproutPageVMFactory
    {
        SproutPageVM Create(SproutPageConfiguration pageConfig, object? parameter);
    }
}