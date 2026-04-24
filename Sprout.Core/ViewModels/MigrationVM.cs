using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Migration;
using System.Diagnostics;
using System.IO;

namespace Sprout.Core.ViewModels
{
    public partial class MigrationVM : ObservableObject
    {
        public string Title => "Migrations";

        private readonly ISqlServerMigrationService _migrationService;
        private readonly IConfigurationService _configService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RunMigrationsCommand))]
        private bool _isBusy;

        public MigrationVM(ISqlServerMigrationService migrationService,
            IConfigurationService configService,
            IDialogService dialogService)
        {
            _migrationService = migrationService;
            _configService = configService;
            _dialogService = dialogService;
        }

        [RelayCommand(CanExecute = nameof(CanRun))]
        private async Task RunMigrationsAsync()
        {
            IsBusy = true;

            try
            {
                var result = await _migrationService.RunMigrationsAsync();

                if (result.Exception != null)
                {
                    _dialogService.ShowMessage(result.Exception.ToString(), "Migration Failed", DialogButton.OK, DialogImage.Error);
                }
                else if (result.Error != null)
                {
                    var e = result.Error;
                    var message = $"File: {e.RelativeFilePath}{Environment.NewLine}" +
                                  $"Batch {e.BatchNumber}:{Environment.NewLine}{e.BatchContent}{Environment.NewLine}" +
                                  $"Exception:{Environment.NewLine}{e.ex}";
                    _dialogService.ShowMessage(message, "Migration Failed", DialogButton.OK, DialogImage.Error);
                }
                else
                {
                    var count = result.Executed.Count;
                    var message = count == 0
                        ? "No pending migrations."
                        : $"Successfully executed {count} migration script(s).";
                    _dialogService.ShowMessage(message, "Migrations", DialogButton.OK, DialogImage.None);
                }
            }
            finally
            {
                IsBusy = false;
                OpenLogFile();
            }
        }

        private bool CanRun() => !IsBusy;

        private void OpenLogFile()
        {
            var settings = _configService.Load().Settings;
            var logPath = Path.Combine(settings.MigrationsFolder, "sprout_migrations.log");

            if (!File.Exists(logPath))
                return;

            Process.Start(new ProcessStartInfo(logPath) { UseShellExecute = true });
        }
    }
}
