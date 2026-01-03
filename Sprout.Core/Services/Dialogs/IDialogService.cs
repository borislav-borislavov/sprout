using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using System.Collections.ObjectModel;

namespace Sprout.Core.Services.Dialogs
{
    public interface IDialogService
    {
        bool ShowEditMenu(
            ObservableCollection<SproutPageConfiguration> pageConfigs,
            IConfigurationService configService);

        void ShowEditPage(SproutPageConfiguration pageConfig, IConfigurationService configService);
    }
}
