using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.ViewModels;
using Sprout.Core.Views;
using Sprout.Core.Views.Controls;
using Sprout.Tests.Helpers;
using Sprout.Tests.Integration.SqlServer.TestCases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Sprout.Tests.Integration.SqlServer.PageTests
{
    public class PageFilterTests
    {
        private ServiceProvider _serviceProvider;
        private SqlServerService _sqlServerService;
        private string _connectionString = "Data Source=.;Database=SproutTestDb;Integrated Security=True;TrustServerCertificate=True;Command Timeout=0";

        public PageFilterTests()
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
        public async Task TestGridFilterByUserName()
        {
            //Arrange
            await EnsureTestDatabaseExists();
            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var dataGridUsersName = "dgUsers";
            var filterTitle = "UserName";

            var config = new SproutPageConfiguration
            {
                Root = new GridConfig
                {
                    Rows = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],
                    Columns = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],

                    Children =
                    [
                        new SproutDataGridConfig
                        {
                            Name = dataGridUsersName,
                            Column = 0,
                            Row = 0,
                            ColumnSpan = 10,
                            RowSpan = 10,
                            Columns = [
                                new SproutDataGridColumnConfig
                                {
                                    Header = "User Name",
                                    BindingPath = "UserName",
                                    ColumnType = ColumnType.Text,
                                },
                                new SproutDataGridColumnConfig
                                {
                                    Header = "Email",
                                    BindingPath = "Email",
                                    ColumnType = ColumnType.Text,
                                }
                                ],
                            DataAdapter = new SqlServerDataAdapterConfig
                            {
                                Name = dataGridUsersName,
                                ConnectionString = _connectionString,
                                InsertCommand = new SqlServerEditCommandConfig(),
                                UpdateCommand = new SqlServerEditCommandConfig(),
                                DeleteCommand = new SqlServerEditCommandConfig(),
                                DataProvider = new SqlServerDataProviderConfig
                                {
                                    Text = "SELECT ID, UserName, Email FROM Users {!whereFilter}",
                                    FilterConfigs =
                                    [
                                        new FilterConfig
                                        {
                                            Title = filterTitle,
                                            Text = "UserName = {0}",
                                            EditorType = EditorType.TextBox
                                        }
                                    ]
                                }
                            }
                        }
                    ]
                }
            };

            var pageVMFactory = _serviceProvider.GetRequiredService<ISproutPageVMFactory>();
            var sproutPageVM = pageVMFactory.Create(config, null);

            var dataGrid = sproutPageVM.DynamicViewInstance._controls[dataGridUsersName] as SproutDataGrid;

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

                await PumpDispatcherUntilAsync(
                    () => dataGrid.dataGrid.ItemsSource != null,
                    TimeSpan.FromSeconds(15));

                //Assert - unfiltered load returns all users
                var dataView = dataGrid.dataGrid.ItemsSource as DataView;
                Assert.NotNull(dataView);
                Assert.Equal(3, dataView.Count);

                //Act - set the filter value through the filter UI and apply it
                var textFilter = Assert.IsType<SproutDataGridTextFilter>(dataGrid.spFilters.Children[0]);
                textFilter.tbFilterValue.Text = "bob_dev";

                Assert.NotNull(dataGrid.btnApplyFilters.Command);
                dataGrid.btnApplyFilters.Command.Execute(dataGrid.btnApplyFilters.CommandParameter);

                await PumpDispatcherUntilAsync(
                    () => dataGrid.dataGrid.ItemsSource is DataView dv && dv.Count == 1,
                    TimeSpan.FromSeconds(15));

                //Assert - only the filtered record is shown
                var filteredView = dataGrid.dataGrid.ItemsSource as DataView;
                Assert.NotNull(filteredView);
                Assert.Equal(1, filteredView.Count);
                Assert.Equal("bob_dev", filteredView[0]["UserName"]);
                Assert.Equal("bob@example.com", filteredView[0]["Email"]);
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
