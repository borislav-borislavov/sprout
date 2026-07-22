using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using Sprout.Core;
using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Windows;

namespace Sprout.Tests
{
    public class UnitTest1
    {
        private ServiceProvider _serviceProvider;
        private ISproutControlFactory _controlFactory;

        public UnitTest1()
        {
            TestResources.EnsureLoaded();

            var services = new ServiceCollection();
            services.AddCoreServices();

            _serviceProvider = services.BuildServiceProvider();

            _controlFactory = _serviceProvider.GetRequiredService<ISproutControlFactory>();
        }

        [WpfFact]
        public void SproutTextBox_Test_Binding_Dependency()
        {
            //Arrange
            var config = new SproutTextBoxConfig
            {
                Binding = "{@txtTest.Text}"
            };

            //Act
            var control = _controlFactory.GetControl(config, []);

            //Assert
            var sproutTextBox = control as SproutTextBox;
            Assert.NotNull(sproutTextBox);

            Assert.True(sproutTextBox.VM.Dependencies.Count() == 1);
        }

        [WpfFact]
        public void SproutButton_Tests()
        {
            //Arrange
            var config = new SproutButtonConfig
            {
                DataAdapter = new SqlServerDataAdapterConfig
                {
                    ConnectionString = null,
                    DataProvider = new SqlServerDataProviderConfig
                    {
                        Text = "{@txtTest.Text}"
                    }
                }
            };

            //Act
            var control = _controlFactory.GetControl(config, []);

            //Assert
            var sproutButton = control as SproutButton;

            var dataAdapterHost = sproutButton.VM as IDataAdapterHost;
            Assert.NotNull(dataAdapterHost);

            Assert.NotNull(sproutButton);
            Assert.NotNull(sproutButton.VM.DataAdapter.DataProvider.Text);

            AssertDataProvider_Assert_txtTest_Dependency(sproutButton.VM.DataAdapter);
        }

        private void AssertDataProvider_Assert_txtTest_Dependency(IDataAdapter dataAdapter)
        {
            Assert.NotNull(dataAdapter.DataProvider);
            Assert.True(dataAdapter.DataProvider.Dependencies.Count() == 1);
            var dep = dataAdapter.DataProvider.Dependencies.First();
            Assert.True(dep.ControlName == "txtTest");
            Assert.True(dep.PropertyPath == "Text");
        }

        [WpfFact]
        public void SproutCombo_Tests()
        {
            //Arrange
            var config = new SproutComboConfig
            {
                DisplayColumn = "Name",
                ValueColumn = "Id",
                SelectedValue = "{@txtTest.Text}",
                DataAdapter = new SqlServerDataAdapterConfig
                {
                    DataProvider = new SqlServerDataProviderConfig
                    {
                        Text = "SELECT Id, Name FROM Users WHERE Id = {@txtTest.Text}"
                    }
                }
            };

            //Act
            var control = _controlFactory.GetControl(config, []);

            //Assert
            var sproutCombo = control as SproutCombo;
            Assert.NotNull(sproutCombo);

            var dataAdapterHost = sproutCombo.VM as IDataAdapterHost;
            Assert.NotNull(dataAdapterHost);

            AssertDataProvider_Assert_txtTest_Dependency(dataAdapterHost.DataAdapter);
        }

        [WpfFact]
        private void SproutList_Test()
        {
            var config = new SproutListConfig
            {
                DataAdapter = new SqlServerDataAdapterConfig
                {
                    DataProvider = new SqlServerDataProviderConfig
                    {
                        Text = "SELECT Id, Name FROM Users WHERE Id = {@txtTest.Text}"
                    }
                }
            };


            //Act
            var control = _controlFactory.GetControl(config, []);

            //Assert
            var sproutList = control as SproutList;
            Assert.NotNull(sproutList);

            var dataAdapterHost = sproutList.VM as IDataAdapterHost;
            Assert.NotNull(dataAdapterHost);

            AssertDataProvider_Assert_txtTest_Dependency(dataAdapterHost.DataAdapter);
        }
    }
}
