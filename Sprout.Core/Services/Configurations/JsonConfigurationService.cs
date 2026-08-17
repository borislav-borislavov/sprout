using Newtonsoft.Json;
using Sprout.Core.Models.Configurations;
using System.IO;
using System.Text;

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
            var configFilePath = GetConfigFilePath();

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
                var configFilePath = GetConfigFilePath();

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

        private string GetConfigFilePath()
        {
            if (!string.IsNullOrEmpty(_seedPath))
                return _seedPath;

            var seedVaultPath = Path.Combine(Environment.CurrentDirectory, "SeedVault");
            Directory.CreateDirectory(seedVaultPath);
            return Path.Combine(seedVaultPath, "main.seed");
        }
    }
}
