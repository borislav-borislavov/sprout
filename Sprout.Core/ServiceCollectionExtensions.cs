using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Factories;
using Sprout.Core.Services.Configurations;
using Sprout.Core.ViewModels;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddTransient<IConfigurationService, JsonConfigurationService>();
            services.AddTransient<SproutPage>();
            services.AddTransient<IPageFactory, PageFactory>();

            services.AddTransient<MainViewVM>();
            services.AddTransient<MainView>();
        }
    }
}
