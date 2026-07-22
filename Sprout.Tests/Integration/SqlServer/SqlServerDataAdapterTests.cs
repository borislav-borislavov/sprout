using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.SproutControlVMs;
using Sprout.Tests.Integration.SqlServer.TestCases;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Tests.Integration.SqlServer
{
    public class SqlServerDataAdapterTests
    {
        private ServiceProvider _serviceProvider;
        private SqlServerService _sqlServerService;
        private string _connectionString = "Data Source=.;Database=SproutTestDb;Integrated Security=True;TrustServerCertificate=True;Command Timeout=0";

        public SqlServerDataAdapterTests()
        {
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

        [Fact]
        public async Task SimpleSelect()
        {
            await EnsureTestDatabaseExists();

            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var testName = "TestName";
            var config = new SqlServerDataAdapterConfig
            {
                Name = testName,
                ConnectionString = _connectionString,

                DataProvider = new SqlServerDataProviderConfig
                {
                    Text = "SELECT * FROM Users"
                },
            };

            var dataAdapterFactory = _serviceProvider.GetRequiredService<IDataAdapterFactory>();
            var dataAdapter = dataAdapterFactory.Create(config);

            var sqlServerDataAdapter = dataAdapter as SqlServerDataAdapter;
            Assert.NotNull(sqlServerDataAdapter);

            var dataServiceFactory = _serviceProvider.GetRequiredService<IDataServiceFactory>();
            var vmRegistry = new VMRegistry();
            SproutDataGridVM dataGridVM = new(testName);
            dataGridVM.DataAdapter = dataAdapter;
            vmRegistry.Register(dataGridVM);

            using var dataService = dataServiceFactory.Create(dataAdapter, vmRegistry);
            await dataService.ProvideData();

            SQLServerUserTaskTestCase.AssertUsers(sqlServerDataAdapter.DataProvider.Data);
        }

        [Fact]
        public async Task InsertWithMessage()
        {
            await EnsureTestDatabaseExists();

            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var testName = "TestName";
            var config = new SqlServerDataAdapterConfig
            {
                Name = testName,
                ConnectionString = _connectionString,

                DataProvider = new SqlServerDataProviderConfig
                {
                    Text = "SELECT * FROM Users"
                },

                InsertCommand = new SqlServerEditCommandConfig
                {
                    Text =
                        """
                        INSERT INTO dbo.Users (UserName, Email, UserTypeID)
                        VALUES ({@UserName}, {@Email}, {@UserTypeID});

                        INSERT INTO #Messages VALUES ('Info', 'User inserted successfully')
                        """
                },
            };

            var dataAdapterFactory = _serviceProvider.GetRequiredService<IDataAdapterFactory>();
            var dataAdapter = dataAdapterFactory.Create(config);

            var sqlServerDataAdapter = dataAdapter as SqlServerDataAdapter;
            Assert.NotNull(sqlServerDataAdapter);

            var dataServiceFactory = _serviceProvider.GetRequiredService<IDataServiceFactory>();
            var vmRegistry = new VMRegistry();
            SproutDataGridVM dataGridVM = new(testName);
            dataGridVM.DataAdapter = dataAdapter;
            vmRegistry.Register(dataGridVM);

            using var dataService = dataServiceFactory.Create(dataAdapter, vmRegistry);

            await dataService.ProvideData();

            var newRow = sqlServerDataAdapter.DataProvider.Data.NewRow();
            newRow["UserName"] = "dave_new";
            newRow["Email"] = "dave@example.com";
            newRow["UserTypeID"] = 2;

            sqlServerDataAdapter.DataProvider.Data.Rows.Add(newRow);

            var changeResult = await dataService.Insert(newRow);
            Assert.NotNull(changeResult);

            Assert.True(changeResult.Messages.Count == 1);

            await dataService.ProvideData();

            SQLServerUserTaskTestCase.AssertUserInserted(sqlServerDataAdapter.DataProvider.Data);
        }

        [Fact]
        public async Task UpdateWithMessage()
        {
            await EnsureTestDatabaseExists();

            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var testName = "TestName";
            var config = new SqlServerDataAdapterConfig
            {
                Name = testName,
                ConnectionString = _connectionString,

                DataProvider = new SqlServerDataProviderConfig
                {
                    Text = "SELECT * FROM Users"
                },

                UpdateCommand = new SqlServerEditCommandConfig
                {
                    Text =
                        """
                        UPDATE dbo.Users
                        SET UserName = {@UserName}, Email = {@Email}
                        WHERE ID = {@ID};

                        INSERT INTO #Messages VALUES ('Info', 'User updated successfully')
                        """
                },
            };

            var dataAdapterFactory = _serviceProvider.GetRequiredService<IDataAdapterFactory>();
            var dataAdapter = dataAdapterFactory.Create(config);

            var sqlServerDataAdapter = dataAdapter as SqlServerDataAdapter;
            Assert.NotNull(sqlServerDataAdapter);

            var dataServiceFactory = _serviceProvider.GetRequiredService<IDataServiceFactory>();
            var vmRegistry = new VMRegistry();
            SproutDataGridVM dataGridVM = new(testName);
            dataGridVM.DataAdapter = dataAdapter;
            vmRegistry.Register(dataGridVM);

            using var dataService = dataServiceFactory.Create(dataAdapter, vmRegistry);

            await dataService.ProvideData();

            var rowToUpdate = sqlServerDataAdapter.DataProvider.Data.Rows[0];
            rowToUpdate["UserName"] = "alice_updated";
            rowToUpdate["Email"] = "alice.updated@example.com";

            var changeResult = await dataService.Update(rowToUpdate);
            Assert.NotNull(changeResult);

            Assert.True(changeResult.Messages.Count == 1);

            await dataService.ProvideData();

            SQLServerUserTaskTestCase.AssertUserUpdated(sqlServerDataAdapter.DataProvider.Data);
        }

        [Fact]
        public async Task DeleteWithMessage()
        {
            await EnsureTestDatabaseExists();

            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var testName = "TestName";
            var config = new SqlServerDataAdapterConfig
            {
                Name = testName,
                ConnectionString = _connectionString,

                DataProvider = new SqlServerDataProviderConfig
                {
                    Text = "SELECT * FROM Users"
                },

                DeleteCommand = new SqlServerEditCommandConfig
                {
                    Text =
                        """
                        DELETE FROM dbo.Tasks WHERE UserID = {@ID};
                        DELETE FROM dbo.Users WHERE ID = {@ID};

                        INSERT INTO #Messages VALUES ('Info', 'User deleted successfully')
                        """
                },
            };

            var dataAdapterFactory = _serviceProvider.GetRequiredService<IDataAdapterFactory>();
            var dataAdapter = dataAdapterFactory.Create(config);

            var sqlServerDataAdapter = dataAdapter as SqlServerDataAdapter;
            Assert.NotNull(sqlServerDataAdapter);

            var dataServiceFactory = _serviceProvider.GetRequiredService<IDataServiceFactory>();
            var vmRegistry = new VMRegistry();
            SproutDataGridVM dataGridVM = new(testName);
            dataGridVM.DataAdapter = dataAdapter;
            vmRegistry.Register(dataGridVM);

            using var dataService = dataServiceFactory.Create(dataAdapter, vmRegistry);

            await dataService.ProvideData();

            var rowToDelete = sqlServerDataAdapter.DataProvider.Data.Rows[0];

            var changeResult = await dataService.Delete(rowToDelete);
            Assert.NotNull(changeResult);

            Assert.True(changeResult.Messages.Count == 1);

            await dataService.ProvideData();

            SQLServerUserTaskTestCase.AssertUserDeleted(sqlServerDataAdapter.DataProvider.Data);
        }
    }
}
