using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Factories;
using Sprout.Core.Services.ActionMessageService;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Login;
using Sprout.Core.Services.Navigation;
using Sprout.Core.ViewModels;
using Sprout.Core.Views;
using Sprout.Core.Windows;

namespace Sprout.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddTransient<IConfigurationService, JsonConfigurationService>();
            services.AddTransient<INavigationService, NavigationService>();
            services.AddTransient<IDialogService, DialogService>();
            services.AddTransient<IDataAdapterFactory, DataAdapterFactory>();
            services.AddTransient<IDataServiceFactory, DataServiceFactory>();
            services.AddTransient<IActionMessageService, ActionMessageService>();
            services.AddTransient<ILoginService, LoginService>();
            services.AddSingleton<ILoggedInUserService, LoggedInUserService>();
            services.AddTransient<ISproutPageVMFactory, SproutPageVMFactory>();
            
            services.AddTransient<LoginVM>();
            services.AddTransient<MainViewVM>();
            services.AddTransient<MainView>();

            //windows
            //TODO: Splash screen
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
        }
    }
}
