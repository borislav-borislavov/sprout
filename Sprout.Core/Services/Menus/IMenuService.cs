using Sprout.Core.Models.Configurations;
using Sprout.Core.Views;

namespace Sprout.Core.Services.Menus
{
    public interface IMenuService
    {
        MainView MainView { get; }
        void CrerateMenu(SproutConfiguration configuration);
    }
}