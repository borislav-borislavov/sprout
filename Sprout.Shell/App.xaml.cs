using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Sprout.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddCoreServices();

            services.AddTransient<MainWindow>();
            
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<MainWindow>().Show();
        }
    }
}
