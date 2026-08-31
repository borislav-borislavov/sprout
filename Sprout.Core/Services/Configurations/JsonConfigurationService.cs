using Newtonsoft.Json;
using Sprout.Core.Common;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Windows;
using System.IO;
using System.Text;
using System.Windows;

namespace Sprout.Core.Services.Configurations
{
    public class JsonConfigurationService : IConfigurationService
    {
        private readonly string _seedPath;

        private string Passphrase => "EatLessSalt";
        public bool Encrypt { get; set; } = true;

        public JsonConfigurationService(string seedPath)
        {
            _seedPath = seedPath;
        }

        public SproutConfiguration Load()
        {
            var configFilePath = GetSeedFilePath();

            if (!File.Exists(configFilePath)) return new();

            try
            {
                string json = string.Empty;
                if (SeedFileCrypto.IsEncrypted(configFilePath))
                {
                    json = SeedFileCrypto.Decrypt(configFilePath, Passphrase);
                }
                else
                {
                    json = File.ReadAllText(configFilePath, Encoding.UTF8);
                }

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                };

                var debug = JsonConvert.DeserializeObject<SproutConfiguration>(json, settings);

                foreach (var page in debug.Pages)
                {
                    if (page.Root == null) continue;

                    if (page.Root is not GridConfig gridConfig)
                        throw new Exception("For now only the grid is supported as a root");
                }

                return debug;
            }
            catch (Exception ex)
            {
                //TODO: logging
                return new();
            }
        }

        public bool Save(SproutConfiguration sproutConfiguration)
        {
            try
            {
                var configFilePath = GetSeedFilePath();

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(sproutConfiguration, settings);

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
                //TODO: logging
                return false;
            }
        }

        private string GetSeedFilePath()
        {
            if (!string.IsNullOrEmpty(_seedPath))
                return _seedPath;

            var seedVaultPath = Path.Combine(Environment.CurrentDirectory, "SeedVault");
            Directory.CreateDirectory(seedVaultPath);
            var alwaysAsk = File.Exists(Path.Combine(seedVaultPath, "AlwaysAsk.txt"));

            var mainSeed = Path.Combine(seedVaultPath, "main.seed");

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
            var mainSeeds = allSeeds.Where(s => string.Equals(s.FileName, "main.seed", StringComparison.InvariantCultureIgnoreCase));
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
