using Sprout.Core.Features.ButtonActions.GridActions;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views.Controls;
using Sprout.Tests.Helpers;
using System;
using System.Data;
using IDataAdapter = Sprout.Core.Models.DataAdapters.IDataAdapter;

namespace Sprout.Tests.Unit
{
    public class AddRowGridActionTests
    {
        private const string GridName = "dgTest";

        public AddRowGridActionTests()
        {
            TestResources.EnsureLoaded();
        }

        private static (VMRegistry vmRegistry, DataTable data) CreateGrid(SproutDataGridConfig config, DataTable data)
        {
            var dataAdapter = new FakeDataAdapter
            {
                Name = GridName,
                DataProvider = new FakeDataProvider { Data = data }
            };

            var vm = new SproutDataGridVM(GridName, null, null, Guid.NewGuid())
            {
                DataAdapter = dataAdapter
            };

            var grid = new SproutDataGrid
            {
                Name = GridName,
                Config = config,
                VM = vm
            };
            vm.Grid = grid;

            var vmRegistry = new VMRegistry();
            vmRegistry.Register(vm);

            return (vmRegistry, data);
        }

        private static DataTable CreateUsersTable()
        {
            var data = new DataTable();
            data.Columns.Add("UserName", typeof(string));
            data.Columns.Add("Age", typeof(int));
            data.Columns.Add("IsActive", typeof(bool));
            data.Columns.Add("Created", typeof(DateTime));
            return data;
        }

        [WpfFact]
        public async Task AddRow_AppliesConfiguredDefaultValues()
        {
            //Arrange
            var config = new SproutDataGridConfig
            {
                Name = GridName,
                Columns =
                [
                    new SproutDataGridColumnConfig { Header = "User Name", BindingPath = "UserName", DefaultValue = "new user" },
                    new SproutDataGridColumnConfig { Header = "Age", BindingPath = "Age", DefaultValue = "18" },
                    new SproutDataGridColumnConfig { Header = "Active", BindingPath = "IsActive", ColumnType = ColumnType.Check, DefaultValue = "true" },
                    new SproutDataGridColumnConfig { Header = "Created", BindingPath = "Created", ColumnType = ColumnType.Date, DefaultValue = "2024-01-31" }
                ]
            };

            var (vmRegistry, data) = CreateGrid(config, CreateUsersTable());

            //Act
            await new AddRowGridAction(GridName).Perform(vmRegistry, null);

            //Assert
            Assert.Equal(1, data.Rows.Count);
            var row = data.Rows[0];
            Assert.Equal("new user", row["UserName"]);
            Assert.Equal(18, row["Age"]);
            Assert.Equal(true, row["IsActive"]);
            Assert.Equal(new DateTime(2024, 1, 31), row["Created"]);
        }

        [WpfFact]
        public async Task AddRow_ColumnsWithoutDefaultValue_AreLeftUntouched()
        {
            //Arrange
            var config = new SproutDataGridConfig
            {
                Name = GridName,
                Columns =
                [
                    new SproutDataGridColumnConfig { Header = "User Name", BindingPath = "UserName", DefaultValue = "new user" },
                    new SproutDataGridColumnConfig { Header = "Age", BindingPath = "Age" }
                ]
            };

            var (vmRegistry, data) = CreateGrid(config, CreateUsersTable());

            //Act
            await new AddRowGridAction(GridName).Perform(vmRegistry, null);

            //Assert
            Assert.Equal(1, data.Rows.Count);
            var row = data.Rows[0];
            Assert.Equal("new user", row["UserName"]);
            Assert.Equal(DBNull.Value, row["Age"]);
        }

        [WpfFact]
        public async Task AddRow_UnconvertibleDefaultValue_Throws()
        {
            //Arrange
            var config = new SproutDataGridConfig
            {
                Name = GridName,
                Columns =
                [
                    new SproutDataGridColumnConfig { Header = "Age", BindingPath = "Age", DefaultValue = "not a number" }
                ]
            };

            var (vmRegistry, data) = CreateGrid(config, CreateUsersTable());

            //Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => new AddRowGridAction(GridName).Perform(vmRegistry, null));
            Assert.Contains("not a number", ex.Message);
            Assert.Equal(0, data.Rows.Count);
        }

        [WpfFact]
        public async Task AddRow_DefaultValueForMissingColumn_Throws()
        {
            //Arrange
            var config = new SproutDataGridConfig
            {
                Name = GridName,
                Columns =
                [
                    new SproutDataGridColumnConfig { Header = "Missing", BindingPath = "DoesNotExist", DefaultValue = "x" }
                ]
            };

            var (vmRegistry, data) = CreateGrid(config, CreateUsersTable());

            //Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => new AddRowGridAction(GridName).Perform(vmRegistry, null));
            Assert.Contains("DoesNotExist", ex.Message);
            Assert.Equal(0, data.Rows.Count);
        }

        [Fact]
        public void DefaultValue_RoundTripsThroughJsonSerialization()
        {
            //Arrange - same serializer settings as JsonConfigurationService
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            var config = new SproutDataGridConfig
            {
                Name = GridName,
                Columns =
                [
                    new SproutDataGridColumnConfig { Header = "Age", BindingPath = "Age", DefaultValue = "18" }
                ]
            };

            //Act
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(config, settings);
            var roundTripped = Newtonsoft.Json.JsonConvert.DeserializeObject<SproutDataGridConfig>(json, settings);

            //Assert
            Assert.Equal("18", roundTripped.Columns[0].DefaultValue);
        }

        private class FakeDataAdapter : IDataAdapter
        {
            public IDataProvider DataProvider { get; set; }
            public IEditCommand InsertCommand { get; set; }
            public IEditCommand UpdateCommand { get; set; }
            public IEditCommand DeleteCommand { get; set; }
            public Type ParentType { get; set; }
            public string Name { get; set; }
        }

        private class FakeDataProvider : IDataProvider
        {
            public IDataAdapter Parent => null;
            public DataTable Data { get; set; }
            public string Text { get; set; }
            public Dictionary<string, IFilter> Filters { get; set; } = [];
            public bool DeferInitialLoad { get; set; }
            public IEnumerable<DataProviderDependency> Dependencies => [];
            public void DepenencyChanged(DataProviderDependency changedDependency, VMRegistry vmRegistry) { }
        }
    }
}
