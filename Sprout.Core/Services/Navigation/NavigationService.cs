using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
using Sprout.Core.Services.Dialog;
using Sprout.Core.ViewModels;
using Sprout.Core.Windows;

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

        public void ShowManageAdapter(IDataAdapterConfigHost dataAdapterConfigHost)
        {
            var mdw = _serviceProvider.GetRequiredService<ManageDataAdapterWindow>();
            (mdw.DataContext as ManageDataAdapterVM).AdapterConfigHost = dataAdapterConfigHost;
            mdw.ShowDialog();
        }

        public void ShowScriptEditor(BaseCompiler compiler)
        {
            var editor = _serviceProvider.GetRequiredService<ScriptEditor>();
            editor.Initialize(compiler);
            editor.ShowDialog();
        }

        public bool ShowManageUsings(BaseCompiler compiler)
        {
            var window = _serviceProvider.GetRequiredService<ManageUsingsWindow>();
            window.Initialize(compiler);
            return window.ShowDialog() == true;
        }
    }
}
