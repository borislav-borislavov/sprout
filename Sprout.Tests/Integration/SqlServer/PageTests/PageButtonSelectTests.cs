using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.ViewModels;
using Sprout.Core.Views.Controls;
using Sprout.Tests.Helpers;
using Sprout.Tests.Integration.SqlServer.TestCases;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Sprout.Tests.Integration.SqlServer.PageTests
{
    public class PageButtonSelectTests
    {
        private ServiceProvider _serviceProvider;
        private SqlServerService _sqlServerService;
        private string _connectionString = "Data Source=.;Database=SproutTestDb;Integrated Security=True;TrustServerCertificate=True;Command Timeout=0";

        public PageButtonSelectTests()
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
        public async Task TestTextBoxBoundToButtonSelectActionResult()
        {
            //Arrange
            await EnsureTestDatabaseExists();
            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var buttonName = "btnLoadUser";
            var textBoxName = "txtUserName";
            var expectedUserName = "bob_dev";

            var config = new SproutPageConfiguration
            {
                Root = new GridConfig
                {
                    Rows = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],
                    Columns = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],

                    Children =
                    [
                        new SproutButtonConfig
                        {
                            Name = buttonName,
                            Content = "Load User",
                            Column = 0,
                            Row = 0,
                            ColumnSpan = 5,
                            Actions = [new ExecuteSelectActionConfig()],
                            DataAdapter = new SqlServerDataAdapterConfig
                            {
                                Name = buttonName,
                                ConnectionString = _connectionString,
                                InsertCommand = new SqlServerEditCommandConfig(),
                                UpdateCommand = new SqlServerEditCommandConfig(),
                                DeleteCommand = new SqlServerEditCommandConfig(),
                                DataProvider = new SqlServerDataProviderConfig
                                {
                                    Text = $"SELECT UserName, Email FROM Users WHERE UserName = '{expectedUserName}'"
                                }
                            }
                        },
                        new SproutTextBoxConfig
                        {
                            Name = textBoxName,
                            Column = 0,
                            Row = 1,
                            ColumnSpan = 10,
                            Binding = $"{{@{buttonName}.FirstRow.UserName}}"
                        }
                    ]
                }
            };

            var pageVMFactory = _serviceProvider.GetRequiredService<ISproutPageVMFactory>();
            var sproutPageVM = pageVMFactory.Create(config, null);

            var button = sproutPageVM.DynamicViewInstance._controls[buttonName] as SproutButton;
            var textBox = sproutPageVM.DynamicViewInstance._controls[textBoxName] as SproutTextBox;

            var hwndSource = new HwndSource(new HwndSourceParameters("SproutTestHost")
            {
                Width = 800,
                Height = 600
            })
            {
                RootVisual = sproutPageVM.DynamicViewInstance
            };

            try
            {
                sproutPageVM.DynamicViewInstance.Measure(new Size(800, 600));
                sproutPageVM.DynamicViewInstance.Arrange(new Rect(0, 0, 800, 600));
                sproutPageVM.DynamicViewInstance.UpdateLayout();

                Assert.NotNull(button);
                Assert.NotNull(textBox);

                //The textbox has no value before the select action runs.
                Assert.True(string.IsNullOrEmpty(textBox.textBox.Text));

                //Act - press the button to execute the select action
                Assert.NotNull(button.button.Command);
                button.button.Command.Execute(button.button.CommandParameter);

                await PumpDispatcherUntilAsync(
                    () => textBox.textBox.Text == expectedUserName,
                    TimeSpan.FromSeconds(15));

                //Assert - the textbox picked up the value from the select result
                Assert.Equal(expectedUserName, textBox.textBox.Text);
            }
            finally
            {
                hwndSource.Dispose();
            }
        }

        /// <summary>
        /// Pumps the WPF dispatcher, allowing queued operations, async continuations and
        /// binding updates to run, until <paramref name="condition"/> is met or the timeout elapses.
        /// </summary>
        private static async Task PumpDispatcherUntilAsync(Func<bool> condition, TimeSpan timeout)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                // Let queued dispatcher operations (including binding updates) run.
                await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

                if (condition())
                {
                    return;
                }

                // Yield to allow pending async I/O (data loading) continuations to complete.
                await Task.Delay(50);
            }
        }
    }
}
