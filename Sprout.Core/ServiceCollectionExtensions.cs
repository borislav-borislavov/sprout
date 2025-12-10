using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Factories;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Menus;
using Sprout.Core.Services.PageUIs;
using Sprout.Core.Services.Seeds;
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
            services.AddTransient<ISeedService, SeedService>();
            services.AddTransient<IConfigurationService, JsonConfigurationService>();
            services.AddSingleton<IMenuService, MenuService>();
            services.AddTransient<IPageUIService, PageUIService>();
            services.AddTransient<SproutPage>();
            services.AddTransient<IPageFactory, PageFactory>();
        }
    }
}
