using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Features.SeedFileUpdateFeature;
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
        private readonly ISeedUpdaterFactory _seedUpdaterFactory;

        public static string[] SeedUpdateStrategies { get; } = ["None", "AdHoc"];

        [ObservableProperty]
        private string _sqlServerConnectionString;

        [ObservableProperty]
        private string _duckDbConnectionString;

        [ObservableProperty]
        private int _commandTimeout;

        [ObservableProperty]
        private bool _logSqlQueries;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string _selectedSeedUpdateStrategy;

        [ObservableProperty]
        private ObservableObject _selectedSeedUpdateViewModel;

        public SettingsVM(IConfigurationService configService, IDialogService dialogService, ISeedUpdaterFactory seedUpdaterFactory)
        {
            _configService = configService;
            _dialogService = dialogService;
            _seedUpdaterFactory = seedUpdaterFactory;

            Load();
        }

        private void Load()
        {
            var config = _configService.Load();
            var settings = config.Settings;
            SqlServerConnectionString = settings.SqlServerConnectionString;
            DuckDbConnectionString = settings.DuckDbConnectionString;
            CommandTimeout = settings.CommandTimeout;
            LogSqlQueries = settings.LogSqlQueries;
            this.Version = config.Version;

            if (config.SeedUpdateConfig is AdHocSeedUpdateConfig adHocConfig)
            {
                SelectedSeedUpdateStrategy = "AdHoc";
                SelectedSeedUpdateViewModel = new AdHocSeedUpdateVM(adHocConfig);
            }
            else
            {
                SelectedSeedUpdateStrategy = "None";
                SelectedSeedUpdateViewModel = null;
            }
        }

        [RelayCommand]
        private void InitializeStrategy()
        {
            if (SelectedSeedUpdateStrategy == "AdHoc")
            {
                SelectedSeedUpdateViewModel = new AdHocSeedUpdateVM(new AdHocSeedUpdateConfig());
            }
            else
            {
                SelectedSeedUpdateViewModel = null;
            }
        }

        [RelayCommand]
        private async Task PublishStrategy()
        {
            try
            {
                if (SelectedSeedUpdateViewModel is not AdHocSeedUpdateVM adHocVM)
                {
                    _dialogService.ShowError("Select and configure a seed update strategy before publishing.");
                    return;
                }

                var updater = _seedUpdaterFactory.Create(adHocVM.ToConfig());
                await updater.Publish();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (!int.TryParse(this.Version, out _))
            {
                _dialogService.ShowError("Version must be a valid integer.");
                return;
            }

            try
            {
                var config = _configService.Load();
                config.Settings.SqlServerConnectionString = SqlServerConnectionString;
                config.Settings.DuckDbConnectionString = DuckDbConnectionString;
                config.Settings.CommandTimeout = CommandTimeout;
                config.Settings.LogSqlQueries = LogSqlQueries;
                config.Version = this.Version;

                if (SelectedSeedUpdateViewModel is AdHocSeedUpdateVM adHocVM)
                {
                    config.SeedUpdateConfig = adHocVM.ToConfig();
                }
                else
                {
                    config.SeedUpdateConfig = null;
                }

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
