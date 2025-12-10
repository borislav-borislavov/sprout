using Sprout.Core.Models.Configurations;
using System.Windows;

namespace Sprout.Core.Services.PageUIs
{
    public interface IPageUIService
    {
        UIElement CreateUI(SproutPageConfiguration pageConfig);
    }
}