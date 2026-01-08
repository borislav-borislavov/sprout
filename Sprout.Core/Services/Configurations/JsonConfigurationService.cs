using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Configurations.Queries;
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
                            Name = "rootGrid",

                            Columns = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],
                            Rows = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],

                            Children = new System.Collections.ObjectModel.ObservableCollection<SproutControlConfig>
                            {
                                new SproutComboConfig
                                {
                                    Name = "languageSelector",
                                    QueryName = "languages",
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
                                    QueryName = "users",
                                    Column = 0,
                                    Row = 1,
                                    ColumnSpan = 5,
                                    RowSpan = 10,
                                    AllowInsert = true,
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
                                    QueryName = "WebApiLogs",
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

                        Queries = new List<QueryConfig>
                        {
                            new QueryConfig
                            {
                                Name = "users",
                                Text = """
                                SELECT * 
                                FROM Users 
                                WHERE 
                                    LanguageID = COALESCE({@languageSelector.Selected.LanguageID}, LanguageID)
                                
                                """,
                                ConnectionString = "Server=.;Database=ROOrdering;Trusted_Connection=True;TrustServerCertificate=Yes",

                                InsertCommand = new TableOperationCommand
                                {
                                    Text = "INSERT INTO Users (Name, Username, LanguageID) VALUES ({@Name}, {@UserName}, {@LanguageID})",
                                    DefaultValues = new()
                                    {
                                        { "Name", "New User" },
                                        { "UserName", "newuser" }
                                    }
                                },
                                UpdateCommand = new TableOperationCommand
                                {
                                    Text = "UPDATE Users SET Name={@Name}, Username={@Username}, LanguageID={@LanguageID} WHERE UserID = {@UserID}"
                                }
                            },
                            new QueryConfig
                            {
                                Name = "WebApiLogs",
                                Text = "SELECT * FROM WebApiLogs WHERE UserID = {@users.Selected.UserID}",
                                ConnectionString = "Server=.;Database=ROOrdering;Trusted_Connection=True;TrustServerCertificate=Yes",
                            },
                            new QueryConfig
                            {
                                Name = "languages",
                                Text = """
                                SELECT CAST(NULL AS INT) LanguageID, 'All' AS Name
                                UNION ALL
                                SELECT * FROM Languages
                                """,
                                ConnectionString = "Server=.;Database=ROOrdering;Trusted_Connection=True;TrustServerCertificate=Yes",
                            }
                        }
                    },
                    new SproutPageConfiguration
                    {
                        Title = "Page2",
                        Root = new GridConfig
                        {
                            Name = "rootGrid",

                            Columns = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],
                            Rows = [ "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*", "1*"],

                            Children = new System.Collections.ObjectModel.ObservableCollection<SproutControlConfig>
                            {
                                new SproutDataGridConfig
                                {
                                    Name = "users",
                                    QueryName = "users",
                                    Column = 0,
                                    Row = 1,
                                    ColumnSpan = 5,
                                    RowSpan = 10,
                                    AllowInsert = true,
                                    Columns = [
                                        new(){ BindingPath = "UserID", Header = "UserID" },
                                        new(){ BindingPath = "Name", Header = "Name" },
                                        new(){ BindingPath = "UserName", Header = "UserName" },
                                        new(){ BindingPath = "LanguageID", Header = "LanguageID" },
                                        ]
                                },
                            }
                        },

                        Queries = new List<QueryConfig>
                        {
                            new QueryConfig
                            {
                                Name = "users",
                                Text = """
                                SELECT * 
                                FROM Users 
                                """,
                                ConnectionString = "Server=.;Database=ROOrdering;Trusted_Connection=True;TrustServerCertificate=Yes",

                                InsertCommand = new TableOperationCommand
                                {
                                    Text = "INSERT INTO Users (Name, Username, LanguageID) VALUES ({@Name}, {@UserName}, {@LanguageID})",
                                    DefaultValues = new()
                                    {
                                        { "Name", "New User" },
                                        { "UserName", "newuser" }
                                    }
                                },
                                UpdateCommand = new TableOperationCommand
                                {
                                    Text = "UPDATE Users SET Name={@Name}, Username={@Username}, LanguageID={@LanguageID} WHERE UserID = {@UserID}"
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
                    typeof(SproutControlConfig).IsAssignableFrom(t) &&   // implements the interface
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
                        if (ti.Type == typeof(SproutControlConfig))
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
