using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Common;
using Sprout.Core.Features.AppStateFeature;
using Sprout.Core.Features.LogFeature;
using Sprout.Core.Features.SproutAppFeature;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Jobs;
using Sprout.Core.Services.Navigation;
using System.Linq;
using System.IO;
using System.Windows;

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

            EnsureCurrentDirectory(serviceProvider);

            if (AppArgs.JobId.HasValue)
            {
                var exitCode = StartHeadlessJob(serviceProvider);
                Application.Current.Shutdown(exitCode);
                return;
            }

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

        private static int StartHeadlessJob(ServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger>();

            if (string.IsNullOrWhiteSpace(AppArgs.SeedPath))
            {
                logger.Log("Headless job startup requires an explicit --seed path.");
                return 2;
            }

            if (AppArgs.JobIdParseFailed || !AppArgs.JobId.HasValue)
            {
                logger.Log($"Invalid --job value '{AppArgs.JobIdRaw}'. Expected a job GUID.");
                return 2;
            }

            var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
            SproutConfiguration config;

            try
            {
                config = configurationService.LoadSpecific(AppArgs.SeedPath);
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to load seed '{AppArgs.SeedPath}' for headless job execution: {ex}");
                return 2;
            }

            var jobId = AppArgs.JobId.Value;
            if (config.Jobs.All(j => j.ID != jobId))
            {
                logger.Log($"Job '{jobId}' was not found in seed '{AppArgs.SeedPath}'.");
                return 2;
            }

            var scheduler = serviceProvider.GetRequiredService<IJobScheduler>();

            try
            {
                scheduler.RunAsync(jobId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Log($"Job '{jobId}' failed during execution: {ex}");
                return 1;
            }

            var status = scheduler.GetStatus(jobId);
            if (status.State == JobRunState.Failed)
            {
                logger.Log($"Job '{jobId}' completed with failure: {status.LastError}");
                return 1;
            }

            logger.Log($"Job '{jobId}' completed successfully.");
            return 0;
        }

        /// <summary>
        /// Ensures that the current directory is set to the directory of the executable file.
        /// This is important for scenarios where the application may be started from a different working directory, which can lead to issues with file paths.
        /// By setting the current directory to the executable's directory, we ensure that relative paths are always resolved the same way.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private static void EnsureCurrentDirectory(ServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger>();

            try
            {
                var exeFilePath = serviceProvider.GetRequiredService<ISproutAppService>().GetExeFilePath();
                var exeDirectory = Path.GetDirectoryName(exeFilePath);
                if (Environment.CurrentDirectory != exeDirectory)
                {
                    Environment.CurrentDirectory = exeDirectory ?? Environment.CurrentDirectory;
                }
            }
            catch (Exception ex)
            {
                logger?.Log($"{nameof(EnsureCurrentDirectory)}: Failed to set current directory: {ex.Message}");
            }
        }
    }
}
