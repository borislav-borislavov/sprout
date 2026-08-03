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

                //var comboCol = dataGrid.dataGrid.Columns[2] as DataGridComboBoxColumn;
                //Assert.NotNull(comboCol.ItemsSource);

                //this section tests the combo box column in the data grid
                var comboDisplayText = dataGrid.dataGrid.ComboDisplayText(0, 2);
                Assert.Equal("Administrator", comboDisplayText); 
            }
            finally
            {
                hwndSource.Dispose();
            }
        }

        [WpfFact]
        public async Task TestTextBoxTwoWayBindingToGridSelectedRow()
        {
            //Arrange
            await EnsureTestDatabaseExists();
            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var dataGridTasksName = "dgTasks";
            var textBoxName = "txtDescription";

            var config = new SproutPageConfiguration
            {
                Root = new GridConfig
                {
                    Rows = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],
                    Columns = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],

                    Children =
                    [
                        new SproutTextBoxConfig
                        {
                            Name = textBoxName,
                            Column = 0,
                            Row = 0,
                            ColumnSpan = 10,
                            Binding = "{@dgTasks.Selected.TaskDescription}",
                            TwoWayBinding = true
                        },
                        new SproutDataGridConfig
                        {
                            Name = dataGridTasksName,
                            Column = 0,
                            Row = 1,
                            ColumnSpan = 10,
                            RowSpan = 9,
                            Columns = [
                                new SproutDataGridColumnConfig
                                {
                                    Header = "Description",
                                    BindingPath = "TaskDescription",
                                    ColumnType = ColumnType.Text,
                                }
                                ],
                            DataAdapter = new SqlServerDataAdapterConfig
                            {
                                Name = dataGridTasksName,
                                ConnectionString = _connectionString,
                                InsertCommand = new SqlServerEditCommandConfig(),
                                UpdateCommand = new SqlServerEditCommandConfig(),
                                DeleteCommand = new SqlServerEditCommandConfig(),
                                DataProvider = new SqlServerDataProviderConfig
                                {
                                    Text = "SELECT ID, TaskDescription FROM Tasks"
                                }
                            }
                        }
                    ]
                }
            };

            var pageVMFactory = _serviceProvider.GetRequiredService<ISproutPageVMFactory>();
            var sproutPageVM = pageVMFactory.Create(config, null);

            var textBox = sproutPageVM.DynamicViewInstance._controls[textBoxName] as SproutTextBox;
            var dataGrid = sproutPageVM.DynamicViewInstance._controls[dataGridTasksName] as SproutDataGrid;

            //var window = new Window
            //{
            //    Content = sproutPageVM.DynamicViewInstance,
            //    DataContext = sproutPageVM,
            //    Width = 800,
            //    Height = 600,
            //};

            //window.ShowDialog();

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

                var dataView = dataGrid.dataGrid.ItemsSource as DataView;
                Assert.NotNull(dataView);
                Assert.True(dataView.Count > 0);

                //Act 1 - select a row and verify the text box picks up the value (one-way pull)
                dataGrid.dataGrid.SelectedItem = dataView[0];

                await PumpDispatcherUntilAsync(
                    () => textBox.textBox.Text == (string)dataView[0]["TaskDescription"],
                    TimeSpan.FromSeconds(5));

                Assert.Equal((string)dataView[0]["TaskDescription"], textBox.textBox.Text);

                //Act 2 - type into the text box and verify the change flows back to the grid row (two-way push)
                var newDescription = "Updated via two-way binding";
                textBox.textBox.Text = newDescription;

                await PumpDispatcherUntilAsync(
                    () => (string)dataView[0]["TaskDescription"] == newDescription,
                    TimeSpan.FromSeconds(5));

                //Assert
                Assert.Equal(newDescription, (string)dataView[0]["TaskDescription"]);
                Assert.Equal(newDescription, (string)dataView[0].Row["TaskDescription"]);
            }
            finally
            {
                hwndSource.Dispose();
            }
        }

        [WpfFact]
        public async Task TestLabelBindingToGridSelectedRow()
        {
            //Arrange
            await EnsureTestDatabaseExists();
            await SQLServerUserTaskTestCase.Create(_sqlServerService);
            var dataGridTasksName = "dgTasks";
            var labelName = "lblDescription";

            var config = new SproutPageConfiguration
            {
                Root = new GridConfig
                {
                    Rows = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],
                    Columns = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],

                    Children =
                    [
                        new SproutLabelConfig
                        {
                            Name = labelName,
                            Column = 0,
                            Row = 0,
                            ColumnSpan = 10,
                            Binding = "{@dgTasks.Selected.TaskDescription}"
                        },
                        new SproutDataGridConfig
                        {
                            Name = dataGridTasksName,
                            Column = 0,
                            Row = 1,
                            ColumnSpan = 10,
                            RowSpan = 9,
                            Columns = [
                                new SproutDataGridColumnConfig
                                {
                                    Header = "Description",
                                    BindingPath = "TaskDescription",
                                    ColumnType = ColumnType.Text,
                                }
                                ],
                            DataAdapter = new SqlServerDataAdapterConfig
                            {
                                Name = dataGridTasksName,
                                ConnectionString = _connectionString,
                                InsertCommand = new SqlServerEditCommandConfig(),
                                UpdateCommand = new SqlServerEditCommandConfig(),
                                DeleteCommand = new SqlServerEditCommandConfig(),
                                DataProvider = new SqlServerDataProviderConfig
                                {
                                    Text = "SELECT ID, TaskDescription FROM Tasks"
                                }
                            }
                        }
                    ]
                }
            };

            var pageVMFactory = _serviceProvider.GetRequiredService<ISproutPageVMFactory>();
            var sproutPageVM = pageVMFactory.Create(config, null);

            var label = sproutPageVM.DynamicViewInstance._controls[labelName] as SproutLabel;
            var dataGrid = sproutPageVM.DynamicViewInstance._controls[dataGridTasksName] as SproutDataGrid;

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

                var dataView = dataGrid.dataGrid.ItemsSource as DataView;
                Assert.NotNull(dataView);
                Assert.True(dataView.Count > 1);

                //Act 1 - select a row and verify the label picks up the value
                dataGrid.dataGrid.SelectedItem = dataView[0];

                await PumpDispatcherUntilAsync(
                    () => label.textBlock.Text == (string)dataView[0]["TaskDescription"],
                    TimeSpan.FromSeconds(5));

                Assert.Equal((string)dataView[0]["TaskDescription"], label.textBlock.Text);

                //Act 2 - select another row and verify the label follows the selection
                dataGrid.dataGrid.SelectedItem = dataView[1];

                await PumpDispatcherUntilAsync(
                    () => label.textBlock.Text == (string)dataView[1]["TaskDescription"],
                    TimeSpan.FromSeconds(5));

                Assert.Equal((string)dataView[1]["TaskDescription"], label.textBlock.Text);

                //Act 3 - change the selected row's value and verify the label reacts
                var newDescription = "Updated task description";
                dataView[1]["TaskDescription"] = newDescription;

                await PumpDispatcherUntilAsync(
                    () => label.textBlock.Text == newDescription,
                    TimeSpan.FromSeconds(5));

                //Assert
                Assert.Equal(newDescription, label.textBlock.Text);
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
