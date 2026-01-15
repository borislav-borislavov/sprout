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

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                };

                var debug = JsonConvert.DeserializeObject<SproutConfiguration>(json, settings);

                foreach (var page in debug.Pages)
                {
                    if (page.Root == null) continue;

                    if (page.Root is not GridConfig gridConfig) continue;

                    SetNavigationProperties(gridConfig, page);
                }

                return debug;
            }
            catch (Exception ex)
            {
                //TODO: logging
                return GetSproutConfiguration();
            }
        }

        private void SetNavigationProperties(GridConfig gridConfig, SproutPageConfiguration page)
        {
            foreach (var child in gridConfig.Children)
            {
                //child.Parent = gridConfig;
                //if (child is GridConfig childGrid)
                //{
                //    SetNavigationProperties(childGrid);
                //}
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
                            Name = "rootGrid",

                            Columns = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],
                            Rows = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],

                            Children = new System.Collections.ObjectModel.ObservableCollection<SproutControlConfig>
                            {
                                new SproutComboConfig
                                {
                                    Name = "languageSelector",
                                    DataAdapter = new SqlServerDataAdapterConfig
                                    {
                                        ConnectionString = "Server=.;Database=ROOrdering;Trusted_Connection=True;TrustServerCertificate=Yes",

                                        DataProvider = new SqlServerDataProviderConfig
                                        {
                                            Text = """
													SELECT CAST(NULL AS INT) LanguageID, 'All' AS Name
													UNION ALL
													SELECT * FROM Languages
													"""
                                        }
                                    },

                                    Column = 0,
                                    ColumnSpan = 2,
                                    Row = 0,
                                    DisplayColumn = "Name",
                                    ValueColumn = "LanguageID",
                                    VerticalAlignment = "top"
                                },

                                new SproutDataGridConfig
                                {
                                    Name = "users",
                                    DataAdapter = new SqlServerDataAdapterConfig
                                    {
                                        ConnectionString = "Server=.;Database=ROOrdering;Trusted_Connection=True;TrustServerCertificate=Yes",

                                        DataProvider = new SqlServerDataProviderConfig
                                        {
                                            Text = "SELECT * FROM Users WHERE LanguageID = COALESCE({@languageSelector.Selected.LanguageID}, LanguageID)",
                                        },

                                        InsertCommand = new SqlServerEditCommandConfig
                                        {
                                            Text = "INSERT INTO Users (Name, Username, LanguageID) VALUES ({@Name}, {@UserName}, {@LanguageID})",
                                        },
                                        UpdateCommand = new SqlServerEditCommandConfig
                                        {
                                            Text = "UPDATE Users SET Name={@Name}, Username={@Username}, LanguageID={@LanguageID} WHERE UserID = {@UserID}"
                                        }
                                    },

                                    Column = 0,
                                    Row = 1,
                                    ColumnSpan = 5,
                                    RowSpan = 10,
                                    Columns = [
                                        new(){ BindingPath = "UserID", Header = "UserID" },
                                        new(){ BindingPath = "Name", Header = "Name" },
                                        new(){ BindingPath = "UserName", Header = "UserName" },
                                        new(){ BindingPath = "LanguageID", Header = "LanguageID" },
                                        ]
                                },

                                new SproutDataGridConfig
                                {
                                    Name = "WebApiLogs",
                                    DataAdapter = new SqlServerDataAdapterConfig
                                    {
                                        ConnectionString = "Server=.;Database=ROOrdering;Trusted_Connection=True;TrustServerCertificate=Yes",

                                        DataProvider = new SqlServerDataProviderConfig
                                        {
                                            Text = "SELECT * FROM WebApiLogs WHERE UserID = {@users.Selected.UserID}",
                                        }
                                    },

                                    Column = 5,
                                    ColumnSpan = 5,
                                    Row = 0,
                                    RowSpan = 10,
                                    Columns = [
                                        new(){ BindingPath = "ID", Header = "ID" },
                                        new(){ BindingPath = "Message", Header = "Message" },
                                        new(){ BindingPath = "Route", Header = "Route" },
                                        ]
                                }
                            }
                        },
                    },
                }
            };
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions();
            options.AllowTrailingCommas = true;
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = true;

            var controlConfigTypes = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t =>
                    typeof(SproutControlConfig).IsAssignableFrom(t) &&   // implements the interface
                    t.IsClass &&                            // is a class
                    !t.IsAbstract);                         // not abstract


            var derivedTypes = controlConfigTypes
                .Select(type => new JsonDerivedType(type, GetControlName(type)))
                .ToList();

            var polymorphismOptions = new JsonPolymorphismOptions();
            polymorphismOptions.TypeDiscriminatorPropertyName = "$type";

            foreach (var derivedType in derivedTypes)
            {
                polymorphismOptions.DerivedTypes.Add(derivedType);
            }

            #region AdapterConfigPolymorphism
            var dataAdapterConfigTypes = Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t =>
                    typeof(IDataAdapterConfig).IsAssignableFrom(t) &&   // implements the interface
                    t.IsClass &&                            // is a class
                    !t.IsAbstract);                         // not abstract

            var adapterConfigDerivedTypes = dataAdapterConfigTypes
                .Select(type => new JsonDerivedType(type, GetControlName(type)))
                .ToList();

            var adapterConfigPolymorphismOptions = new JsonPolymorphismOptions();
            adapterConfigPolymorphismOptions.TypeDiscriminatorPropertyName = "$adapterType";

            foreach (var derivedType in adapterConfigDerivedTypes)
            {
                adapterConfigPolymorphismOptions.DerivedTypes.Add(derivedType);
            }
            #endregion

            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    ti =>
                    {
                        if (ti.Type == typeof(SproutControlConfig))
                        {
                            ti.PolymorphismOptions = polymorphismOptions;
                        }

                        if (ti.Type == typeof(IDataAdapterConfig))
                        {
                            ti.PolymorphismOptions = adapterConfigPolymorphismOptions;
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
