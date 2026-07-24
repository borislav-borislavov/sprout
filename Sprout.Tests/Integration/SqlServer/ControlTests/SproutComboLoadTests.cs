using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using Sprout.Tests.Integration.SqlServer.TestCases;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Sprout.Tests.Integration.SqlServer.ControlTests
{
    public class SproutComboLoadTests
    {
        private ServiceProvider _serviceProvider;
        private SqlServerService _sqlServerService;
        private string _connectionString = "Data Source=.;Database=SproutTestDb;Integrated Security=True;TrustServerCertificate=True;Command Timeout=0";

        public SproutComboLoadTests()
        {
            TestResources.EnsureLoaded();
            var services = new ServiceCollection();
            services.AddCoreServices();

            _serviceProvider = services.BuildServiceProvider();

            _sqlServerService = new SqlServerService(_connectionString);
        }

        private async Task EnsureTestDatabaseExists()
        {
            var dbService = new SqlServerService("Data Source=.;Database=master;Integrated Security=True;TrustServerCertificate=True;Command Timeout=0");

            await dbService.OpenConnectionAsync();

            var sql =
                """
                IF EXISTS (SELECT * FROM sys.databases WHERE name = 'SproutTestDb')
                BEGIN
                    ALTER DATABASE SproutTestDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE SproutTestDb;
                END;

                CREATE DATABASE SproutTestDb;
                """;

            await dbService.ExecuteAsync(sql);

            await dbService.CloseConnectionAsync();
        }

        [WpfFact]
        public async Task Test()
        {
            //Arrange
            await EnsureTestDatabaseExists();
            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var testName = "TestName";
            var config = new SproutComboConfig
            {
                Name = testName,
                DisplayColumn = "UserName",
                ValueColumn = "ID",
                SelectedValue = "0",
                DataAdapter = new SqlServerDataAdapterConfig
                {
                    Name = testName,
                    ConnectionString = _connectionString,
                    DataProvider = new SqlServerDataProviderConfig
                    {
                        Text = "SELECT ID, UserName FROM Users"
                    }
                }
            };

            var vmRegistry = new VMRegistry();

            //Act
            var control = _serviceProvider.GetRequiredService<ISproutControlFactory>().GetControl(config, [], vmRegistry);

            //Assert
            var combo = control as SproutCombo;
            Assert.NotNull(combo);

            var dataServiceFactory = _serviceProvider.GetRequiredService<IDataServiceFactory>();
            
            vmRegistry.Register(combo.VM);

            using var dataService = dataServiceFactory.Create(combo.VM.DataAdapter, vmRegistry);
            await dataService.ProvideData();

            Assert.NotNull(combo.comboBox.ItemsSource);
            Assert.NotNull(combo.comboBox.SelectedItem);
        }
    }
}
