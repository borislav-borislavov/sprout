using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Common;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Navigation;
using Sprout.Core.Services.Jobs;
using System.Windows;
using Sprout.Core.Features.LogFeature;

namespace Sprout.Core
{
    public static class SproutApp
    {
        public static void Start()
        {
            AppArgs.Parse();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var services = new ServiceCollection();
            services.AddCoreServices();

            var serviceProvider = services.BuildServiceProvider();
            Application.Current.Exit += (_, _) => serviceProvider.Dispose();

            var logger = serviceProvider.GetRequiredService<ILogger>();
            logger.Log("Application started.");
            Application.Current.DispatcherUnhandledException += (s, e) => logger.Log($"[Dispatcher] {e.Exception}");
            AppDomain.CurrentDomain.UnhandledException += (s, e) => logger.Log($"[AppDomain] {e.ExceptionObject}");
            TaskScheduler.UnobservedTaskException += (s, e) => logger.Log($"[TaskScheduler] {e.Exception}");

            //This line makes sure that the ConfigurationService loads before the JobSchedule singleton locks it in.
            //This is problematic because in some cases it will ask the user to pick a .seed but the dialog can't be displayed from a non STA trhead
            //which leads to all jobs to always fail. Calling this first allowes the ConfigurationService to load and cache its seed file path from a STA thread.
            serviceProvider.GetRequiredService<IConfigurationService>().Load();

            serviceProvider.GetRequiredService<IJobScheduler>().Start();

            var navigationService = serviceProvider.GetRequiredService<INavigationService>();

            var sproutConfig = serviceProvider.GetRequiredService<IConfigurationService>().Load();

            if (sproutConfig.Login is LoginConfiguration loginConfig
                && loginConfig.IsEnabled
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
