using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Login;
using Sprout.Core.Services.Navigation;
using Sprout.Core.Windows;
using System;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sprout.Core
{
    public static class SproutApp
    {
        public static void Start()
        {
            var services = new ServiceCollection();
            services.AddCoreServices();

            var serviceProvider = services.BuildServiceProvider();

            var navigationService = serviceProvider.GetRequiredService<INavigationService>();

            var sproutConfig = serviceProvider.GetRequiredService<IConfigurationService>().Load();

            if (sproutConfig.Login is LoginConfiguration loginConfig
                && loginConfig.DataAdapter != null)
            {
                navigationService.ShowLogin();
            }
            else
            {
                navigationService.ShowMainDashboard();
            }
        }
    }
}
