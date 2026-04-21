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
            var addControl = _serviceProvider.GetRequiredService<AddControl>();
            addControl.ShowDialog();

            return (addControl.DataContext as AddControlVM).NewControl;
        }

        public bool ShowEditMenu()
        {
            var editMenu = _serviceProvider.GetRequiredService<EditMenu>();
            editMenu.ShowDialog();
            return editMenu.ViewModel.IsSaved;
        }

        public void ShowEditPage(SproutPageConfiguration pageConfig, IConfigurationService configService, IDialogService dialogService)
        {
            var editPage = _serviceProvider.GetRequiredService<EditPage>();
            editPage.InitializeVM(pageConfig);
            editPage.ShowDialog();
        }

        public bool ShowEditLoginConfig()
        {
            var editLoginConfig = _serviceProvider.GetRequiredService<EditLoginConfig>();
            editLoginConfig.ShowDialog();
            return editLoginConfig.ViewModel.IsSaved;
        }
    }
}
