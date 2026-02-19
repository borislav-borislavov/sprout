using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Factories;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Navigation;
using Sprout.Core.ViewModels;
using Sprout.Core.Views;

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

            services.AddTransient<MainViewVM>();
            services.AddTransient<MainView>();
        }
    }
}
