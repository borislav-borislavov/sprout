using Newtonsoft.Json;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Sprout.Core.Services.Configurations
{
    public class JsonConfigurationService : IConfigurationService
    {
        private readonly string _seedPath;

        public JsonConfigurationService(string seedPath)
        {
            _seedPath = seedPath;
        }

        public SproutConfiguration Load()
        {
            return LoadFromJson();
        }

        private string GetConfigFilePath()
        {
            if (!string.IsNullOrEmpty(_seedPath))
                return _seedPath;

            var seedVaultPath = Path.Combine(Environment.CurrentDirectory, "SeedVault");
            Directory.CreateDirectory(seedVaultPath);
            return Path.Combine(seedVaultPath, "main.seed");
        }

        private SproutConfiguration LoadFromJson()
        {
            var configFilePath = GetConfigFilePath();

            if (!File.Exists(configFilePath))
            {
                return GetSproutConfiguration();
            }

            try
            {
                var json = File.ReadAllText(configFilePath, Encoding.UTF8);

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
                return GetSproutConfiguration();
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

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(sproutConfiguration, settings);
                
                File.WriteAllText(configFilePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                //TODO: logging
                return false;
            }
        }

        private SproutConfiguration GetSproutConfiguration()
        {
            return new SproutConfiguration();
        }
    }
}
