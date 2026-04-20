using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.ViewModels;
using Sprout.Core.Windows;
using System.Collections.ObjectModel;

namespace Sprout.Core.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowMainDashboard()
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        public void ShowLogin()
        {
            var mainWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            mainWindow.Show();
        }

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

        public bool ShowEditLoginConfig(IConfigurationService configService, IDialogService dialogService)
        {
            var vm = new EditLoginConfigVM(configService, dialogService);
            var editLoginConfig = new EditLoginConfig { DataContext = vm };
            editLoginConfig.ShowDialog();

            return vm.IsSaved;
        }
    }
}
