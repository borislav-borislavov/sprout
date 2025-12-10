using Sprout.Core.Models.Configurations;
using Sprout.Core.Views;

namespace Sprout.Core.Factories
{
    public interface IPageFactory
    {
        SproutPage Create(SproutPageConfiguration pageConfig);
    }
}