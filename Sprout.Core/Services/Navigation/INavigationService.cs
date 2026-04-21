using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using System.Collections.ObjectModel;

namespace Sprout.Core.Services.Navigation
{
    public interface INavigationService
    {
        SproutControlConfig ShowAddControl();

        bool ShowEditMenu();

        void ShowEditPage(SproutPageConfiguration pageConfig, IConfigurationService configService, IDialogService dialogService);

        bool ShowEditLoginConfig();

        void ShowMainDashboard();
        void ShowLogin();
    }
}
