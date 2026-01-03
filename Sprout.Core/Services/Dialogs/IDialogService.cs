using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;

namespace Sprout.Core.Services.Dialogs
{
    public interface IDialogService
    {
        void ShowEditPage(SproutPageConfiguration pageConfig, IConfigurationService configService);
    }
}
