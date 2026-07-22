using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.ViewModels;
using Sprout.Core.Views.Controls;
using Sprout.Tests.Helpers;
using Sprout.Tests.Integration.SqlServer.TestCases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Sprout.Tests.Integration.SqlServer.PageTests
{
    public class PageTest
    {
        private ServiceProvider _serviceProvider;
        private SqlServerService _sqlServerService;
        private string _connectionString = "Data Source=.;Database=SproutTestDb;Integrated Security=True;TrustServerCertificate=True;Command Timeout=0";

        public PageTest()
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
        public async Task TestComboDependentGrid()
        {
            //Arrange
            await EnsureTestDatabaseExists();
            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var comboName = "cmbUserTypes";
            var dataGridUsersName = "dgUsers";

            var config = new SproutPageConfiguration
            {
                Root = new GridConfig
                {
                    Rows = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],
                    Columns = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],

                    Children =
                    [
                        new SproutComboConfig
                        {
                            Name = comboName,
                            ValueColumn = "ID",
                            DisplayColumn = "Name",
                            SelectedValue = "0",
                            DataAdapter = new SqlServerDataAdapterConfig
                            {
                                Name = comboName,
                                ConnectionString = _connectionString,
                                DataProvider = new SqlServerDataProviderConfig
                                {
                                    Text = "SELECT ID, Name FROM UserTypes"
                                }
                            }
                        },
                        new SproutDataGridConfig
                        {
                            Name = dataGridUsersName,
                            Column = 0,
                            Row = 1,
                            ColumnSpan = 10,
                            RowSpan = 9,
                            Columns = [
                                new SproutDataGridColumnConfig
                                {
                                    Header = "ID",
                                    BindingPath = "ID",
                                    ColumnType = ColumnType.Text,
                                },
                                new SproutDataGridColumnConfig
                                {
                                    Header = "User Name",
                                    BindingPath = "UserName",
                                    ColumnType = ColumnType.Text,
                                },
                                new SproutDataGridColumnConfig
                                {
                                    Header = "User Type",
                                    BindingPath = "UserTypeID",
                                    ColumnType = ColumnType.Combo,
                                    DisplayColumn = "Name",
                                    ValueColumn = "ID",
                                    DataAdapter = new SqlServerDataAdapterConfig
                                    {
                                        Name = comboName,
                                        ConnectionString = _connectionString,
                                        DataProvider = new SqlServerDataProviderConfig
                                        {
                                            Text = "SELECT ID, Name FROM UserTypes"
                                        }
                                    },
                                    ComboAdapterKey = comboName
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
                                    Text = "SELECT * FROM Users WHERE UserTypeID = {@cmbUserTypes.Selected.ID}"
                                }
                            }
                        }
                    ]
                }
            };

            var pageVMFactory = _serviceProvider.GetRequiredService<ISproutPageVMFactory>();
            var sproutPageVM = pageVMFactory.Create(config, null);

            //var window = new Window
            //{
            //    Content = sproutPageVM.DynamicViewInstance,
            //    DataContext = sproutPageVM,
            //    Width = 800,
            //    Height = 600,
            //};

            //window.ShowDialog();

            var combo = sproutPageVM.DynamicViewInstance._controls[comboName] as SproutCombo;
            var dataGrid = sproutPageVM.DynamicViewInstance._controls[dataGridUsersName] as SproutDataGrid;



            // The page is never shown, so it is not attached to a PresentationSource.
            // Without that, the Loaded logic / async data loading (OnPageInitialize) and
            // the WPF binding engine never run, leaving ItemsSource/SelectedItem empty.
            // Host the view in an invisible HwndSource to connect it to a presentation
            // source (raising Loaded and enabling binding processing) without showing a window.
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
                // Force a layout pass so the visual tree is realized and bindings are attached.
                sproutPageVM.DynamicViewInstance.Measure(new Size(800, 600));
                sproutPageVM.DynamicViewInstance.Arrange(new Rect(0, 0, 800, 600));
                sproutPageVM.DynamicViewInstance.UpdateLayout();

                // Pump the dispatcher so the async data load and binding updates complete.
                await PumpDispatcherUntilAsync(
                    () => combo.comboBox.ItemsSource != null && combo.comboBox.SelectedItem != null && dataGrid.dataGrid.ItemsSource != null,
                    TimeSpan.FromSeconds(15));

                //Assert
                Assert.NotNull(combo.comboBox.ItemsSource);
                Assert.NotNull(combo.comboBox.SelectedItem);
                Assert.NotNull(dataGrid.dataGrid.ItemsSource);

                var dataView = dataGrid.dataGrid.ItemsSource as DataView;
                Assert.NotNull(dataView);

                Assert.Equal(1, dataView.Count);
                Assert.Equal("alice_admin", dataView[0]["UserName"]);

                var comboCol = dataGrid.dataGrid.Columns[2] as DataGridComboBoxColumn;
                Assert.NotNull(comboCol.ItemsSource);

                //this section tests the combo box column in the data grid
                var comboDisplayText = dataGrid.dataGrid.ComboDisplayText(0, 2);
                Assert.Equal("Administrator", comboDisplayText); 
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
