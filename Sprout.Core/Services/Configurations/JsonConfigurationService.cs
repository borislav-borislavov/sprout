using Sprout.Core.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        public SproutConfiguration Load()
        {
            //return GetDummy();
            return LoadFromJson();
        }

        private static string GetConfigFilePath()
        {
            return Path.Combine(Environment.CurrentDirectory, "SproutConfig.json");
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

                var debug = JsonSerializer.Deserialize<SproutConfiguration>(json, CreateJsonOptions());

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

                var json = JsonSerializer.Serialize(sproutConfiguration, CreateJsonOptions());
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

        private static SproutConfiguration GetDummy()
        {
            return new SproutConfiguration
            {
                Pages = new List<SproutPageConfiguration>
                {
                    new SproutPageConfiguration
                    {
                        Title = "Home",
                        Root = new GridConfig
                        {
                            Columns = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],
                            Rows = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],

                            Children = new List<SproutConfig>
                            {
                                new ButtonConfig
                                {
                                    Content = "Click Me",
                                    Column = 4,
                                    Row = 4,
                                    ColumnSpan = 2,
                                    RowSpan = 2
                                },

                                new GridConfig
                                {
                                    Columns = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],
                                    Rows = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],
                                }
                            }
                        }
                    }
                }
            };
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions();
            options.AllowTrailingCommas = true;
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = true;

            var configTypes = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t =>
                    typeof(SproutConfig).IsAssignableFrom(t) &&   // implements the interface
                    t.IsClass &&                            // is a class
                    !t.IsAbstract);                         // not abstract


            var derivedTypes = configTypes.Select(type => new JsonDerivedType(type, GetControlName(type))).ToList();

            var polymorphismOptions = new JsonPolymorphismOptions();
            polymorphismOptions.TypeDiscriminatorPropertyName = "$type";

            foreach (var derivedType in derivedTypes)
            {
                polymorphismOptions.DerivedTypes.Add(derivedType);
            }

            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    ti =>
                    {
                        if (ti.Type == typeof(SproutConfig))
                        {
                            ti.PolymorphismOptions = polymorphismOptions;
                        }
                    }
                }
            };

            return options;
        }

        private static string GetControlName(Type type)
        {
            return type.Name.Replace("Config", "").ToLower();
        }
    }
}
