using Sprout.Core.Features.SproutAppFeature;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Sprout.Core.Features.SeedFileUpdateFeature
{
    public class AdHocSeedUpdater : ISeedUpdater
    {
        private readonly IConfigurationService _configurationService;
        private readonly AdHocSeedUpdateConfig _updateConfig;
        private readonly IDialogService _dialogService;
        private readonly ISproutAppService _sproutAppService;

        public AdHocSeedUpdater(IConfigurationService configurationService, AdHocSeedUpdateConfig updateConfig, IDialogService dialogService, ISproutAppService sproutAppService)
        {
            _configurationService = configurationService;
            _updateConfig = updateConfig;
            _dialogService = dialogService;
            _sproutAppService = sproutAppService;
        }

        public async Task Update()
        {
            if (_updateConfig == null) return;
            if (string.IsNullOrEmpty(_updateConfig.FilePath)) return;
            if (!File.Exists(_updateConfig.FilePath))
            {
                _dialogService.ShowMessage($"The specified seed file '{_updateConfig.FilePath}' does not exist.", "File Not Found", DialogButton.OK, DialogImage.Error);
                return;
            }

            var currentConfig = _configurationService.Load();
            var newConfig = await Task.Run(() => _configurationService.LoadSpecific(_updateConfig.FilePath));

            if(!int.TryParse(currentConfig.Version, out int currentVersion))
            {
                _dialogService.ShowMessage($"Failed to parse current seed file version '{currentConfig.Version}'.", "Error", DialogButton.OK, DialogImage.Error);
                return;
            }

            if(!int.TryParse(newConfig.Version, out int newVersion))
            {
                _dialogService.ShowMessage($"Failed to parse new seed file version '{newConfig.Version}'.", "Error", DialogButton.OK, DialogImage.Error);
                return;
            }

            if (newVersion <= currentVersion) return;

            if (_dialogService.ShowMessage(
                $"A new version of the seed file is available. Do you want to update from version {currentVersion} to {newVersion}?",
                "Confirm Update",
                DialogButton.YesNo,
                DialogImage.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                //backup the current seed file before updating
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFilePath = _configurationService.GetIdentifier() + $"_v{currentVersion}_{timestamp}.bak";
                File.Copy(_configurationService.GetIdentifier(), backupFilePath, true);

                //update the seed file with the new version
                File.Copy(_updateConfig.FilePath, _configurationService.GetIdentifier(), true);

                _sproutAppService.StartApp();
                _sproutAppService.CloseApp(true);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Failed to update seed file: {ex.Message}", "Update Failed", DialogButton.OK, DialogImage.Error);
            }
        }
    }
}
