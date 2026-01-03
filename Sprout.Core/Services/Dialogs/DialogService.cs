using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.ViewModels;
using Sprout.Core.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Services.Dialogs
{
    public class DialogService : IDialogService
    {
        public void ShowEditMenu(ObservableCollection<SproutPageConfiguration> pageConfigs, IConfigurationService configService)
        {
            var vm = new EditMenuVM(configService);
            var editMenu = new EditMenu { DataContext = vm };
            editMenu.ShowDialog();
        }

        public void ShowEditPage(SproutPageConfiguration pageConfig, IConfigurationService configService)
        {
            var vm = new EditPageVM(configService);
            var editPage = new EditPage { DataContext = vm };
            vm.Initialize(pageConfig);

            editPage.ShowDialog();
        }
    }
}
