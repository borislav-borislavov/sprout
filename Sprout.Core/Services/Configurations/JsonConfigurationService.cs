using Newtonsoft.Json;
using Sprout.Core.Common;
using Sprout.Core.Features.LogFeature;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Windows;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text;
using System.Windows;

namespace Sprout.Core.Services.Configurations
{
    public class JsonConfigurationService : IConfigurationService
    {
        private readonly string _seedPath;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;

        private string Passphrase => "EatLessSalt";
        private static SproutConfiguration? _cachedConfig;

        public bool Encrypt { get; set; } = true;

        public JsonConfigurationService(string seedPath, ILogger logger, IDialogService dialogService)
        {
            _seedPath = seedPath;
            _logger = logger;
            _dialogService = dialogService;
        }

        /// <summary>
        /// Loads a specific file. Does not benefit from caching, file change tracking, seed picker etc.
        /// </summary>
        public SproutConfiguration LoadSpecific(string identifier)
        {
            string json = string.Empty;
            if (SeedFileCrypto.IsEncrypted(identifier))
            {
                json = SeedFileCrypto.Decrypt(identifier, Passphrase);
            }
            else
            {
                json = File.ReadAllText(identifier, Encoding.UTF8);
            }

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            var config = JsonConvert.DeserializeObject<SproutConfiguration>(json, settings) ?? new SproutConfiguration();

            return config;
        }

        #region File changed tracker
        private static long lastUsn = 0;
        private static ulong _lastFrn = 0;
        private static bool _usnCrashed = false;

        private bool IsFileChanged(string filePath)
        {
            //if for some reason this fieature crashed it is better to always load the file so that the app runs properly.
            if (_usnCrashed) return true;

            var result = false;

            try
            {
                using UsnJournalReader _usnJournalReader = new();
                var (usn, fileReferenceNumber, reason) = _usnJournalReader.GetUsnRecord(filePath);

                if (fileReferenceNumber != _lastFrn) // Different file entirely (deleted+recreated, or you're pointing at the wrong file)
                {
                    result = true;
                }
                else if (usn != lastUsn) // Same file, but it changed — check `reason` for what kind of change
                {
                    result = true;
                }

                lastUsn = usn;
                _lastFrn = fileReferenceNumber;

                return result;
            }
            catch (Exception ex)
            {
                _usnCrashed = true;
                _logger.Log($"USN Journal crashed: {ex}");
                return true;
            }
        }
        #endregion

        public SproutConfiguration Load()
        {
            string configFilePath = string.Empty;
            try
            {
                configFilePath = GetIdentifier();

                AppArgs.SeedPath = configFilePath;

                if (!File.Exists(configFilePath)) return new();

                if (!IsFileChanged(configFilePath) && _cachedConfig != null)
                {
                    return _cachedConfig;
                }

                return _cachedConfig = LoadSpecific(configFilePath);
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to load configuration {configFilePath}: {ex}");
                return new();
            }
        }

        public bool Save(SproutConfiguration sproutConfiguration)
        {
            string configFilePath = string.Empty;

            try
            {
                configFilePath = GetIdentifier();

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                };

                var json = JsonConvert.SerializeObject(sproutConfiguration, settings);

                if (Encrypt)
                {
                    SeedFileCrypto.Encrypt(configFilePath, json, Passphrase);
                    return true;
                }

                File.WriteAllText(configFilePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to save configuration {configFilePath}: {ex}");
                return false;
            }
        }

        public string GetIdentifier()
        {
            if (!string.IsNullOrEmpty(_seedPath))
                return _seedPath;

            var seedVaultPath = Path.Combine(Environment.CurrentDirectory, "SeedVault");
            Directory.CreateDirectory(seedVaultPath);
            var alwaysAsk = File.Exists(Path.Combine(seedVaultPath, Const.AlwaysAsk));

            var mainSeed = Path.Combine(seedVaultPath, Const.DefaultSeedFileName);

            if (!alwaysAsk && File.Exists(mainSeed)) return mainSeed;

            var allSeeds = Directory.EnumerateFiles(seedVaultPath, "*.seed", SearchOption.AllDirectories)
                .Select(fp => new SeedFile
                    { 
                        FilePath = fp, 
                        FileName = Path.GetFileName(fp), 
                        RelativeFilePath = fp.Replace(seedVaultPath, "") 
                    });

            //if no seed files exist return the mainSeed to be created
            if (allSeeds.Any() == false) return mainSeed;

            //if there is just one seed file, return it
            if (allSeeds.Count() == 1)
            {
                return allSeeds.First().FilePath;
            }

            //if there is one nested in a folder main seed
            var mainSeeds = allSeeds.Where(s => string.Equals(s.FileName, Const.DefaultSeedFileName, StringComparison.InvariantCultureIgnoreCase));
            if (!alwaysAsk && mainSeeds.Count() == 1)
            {
                return mainSeeds.First().FilePath;
            }

            var seedPicker = new SeedPicker(allSeeds);
            
            if (seedPicker.ShowDialog() == true)
            {
                //from now on the selected seed will be used for the application and the user will not be prompted again until the application is restarted
                AppArgs.SeedPath = seedPicker.SelectedSeed.FilePath;
                return AppArgs.SeedPath;
            }
            else
            {
                //if the user cancels the seed picker, we will exit the application
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
                return null;
            }
        }
    }
}
