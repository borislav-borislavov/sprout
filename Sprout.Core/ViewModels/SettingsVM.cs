using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;

namespace Sprout.Core.ViewModels
{
    public partial class SettingsVM : ObservableObject
    {
        public string Title => "Settings";

        private readonly IConfigurationService _configService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _sqlServerConnectionString;

        [ObservableProperty]
        private int _commandTimeout;

        public SettingsVM(IConfigurationService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;

            Load();
        }

        private void Load()
        {
            var settings = _configService.Load().Settings;
            SqlServerConnectionString = settings.SqlServerConnectionString;
            CommandTimeout = settings.CommandTimeout;
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                var config = _configService.Load();
                config.Settings.SqlServerConnectionString = SqlServerConnectionString;
                config.Settings.CommandTimeout = CommandTimeout;
                _configService.Save(config);
                _dialogService.ShowMessage("Settings saved.", "Settings", DialogButton.OK, DialogImage.None);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }
    }
}
