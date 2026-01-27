using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.ViewModels;
using Sprout.Core.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        public SproutControlConfig ShowAddControl()
        {
            var vm = new AddControlVM();
            var addControl = new AddControl { DataContext = vm };
            addControl.ShowDialog();

            return vm.NewControl;
        }

        public bool ShowEditMenu(ObservableCollection<SproutPageConfiguration> pageConfigs, IConfigurationService configService)
        {
            var vm = new EditMenuVM(configService);
            var editMenu = new EditMenu { DataContext = vm };
            editMenu.ShowDialog();

            return vm.IsSaved;
        }

        public void ShowEditPage(SproutPageConfiguration pageConfig, IConfigurationService configService, IDialogService dialogService)
        {
            var vm = new EditPageVM(configService, this, dialogService);
            var editPage = new EditPage { DataContext = vm };
            vm.Initialize(pageConfig);

            editPage.ShowDialog();
        }
    }
}
