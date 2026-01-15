using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    public partial class EditPageVM : ObservableObject
    {
        public SproutPageConfiguration PageConfig { get; set; }

        [ObservableProperty]
        private IDataAdapterConfig _selectedDataAdapter;

        [ObservableProperty]
        private ObservableCollection<SproutControlConfig> _controls = [];

        [ObservableProperty]
        private SproutControlConfig _selectedNode;
        private readonly IConfigurationService _configService;
        private readonly IDialogService _dialogService;

        public string[] AdapterTypes { get; set; } = ["SqlServer", "SQLite", "Excel"];

        [ObservableProperty]
        private string _selectedAdapterType;

        [ObservableProperty]
        private ObservableObject _selectedAdapterViewModel;

        public EditPageVM(IConfigurationService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;
        }

        public void Initialize(SproutPageConfiguration pageConfig)
        {
            PageConfig = pageConfig;

            if (pageConfig.Root is not GridConfig gridConfig)
            {
                return;
            }

            Controls.Add(gridConfig);
        }

        [RelayCommand]
        private void Save()
        {
            var debug = PageConfig;

            var sproutConfig = _configService.Load();
            var foundPage = sproutConfig.Pages.FirstOrDefault(p => p.Title == PageConfig.Title);
            var pageIndex = sproutConfig.Pages.IndexOf(foundPage);
            sproutConfig.Pages.Remove(foundPage);
            sproutConfig.Pages.Insert(pageIndex, PageConfig);

            _configService.Save(sproutConfig);

        }

        [RelayCommand]
        private void AddControl()
        {
            SproutControlConfig newControl = _dialogService.ShowAddControl();

            if (newControl == null) return;

#warning create interface to mark containers
            if (SelectedNode is GridConfig gridConfig)
            {
                gridConfig.Children.Add(newControl);
            }
        }

        partial void OnSelectedNodeChanged(SproutControlConfig value)
        {
            if (value is IDataAdapterControlConfig dataAdapterControlConfig)
            {
                SelectedDataAdapter = dataAdapterControlConfig.DataAdapter;

                if (dataAdapterControlConfig.DataAdapter is SqlServerDataAdapterConfig sqlServerDataAdapterConfig)
                {
                    SelectedAdapterType = "SqlServer";
                    SelectedAdapterViewModel = new SqlServerDataAdapterVM(sqlServerDataAdapterConfig);
                }
                else 
                {
                    SelectedAdapterType = null;
                    SelectedAdapterViewModel = null;
                }
            }
            else
            {
                SelectedAdapterType = null;
                SelectedAdapterViewModel = null;
            }
        }

        partial void OnSelectedAdapterTypeChanged(string value)
        {
            //if (value == "SqlServer" 
            //    && SelectedQuery is IDataAdapterConfig dataAdapter 
            //    && dataAdapter is SqlServerDataAdapterConfig sqlServerDataAdapterConfig)
            //{
            //    SelectedAdapterViewModel = new SqlServerDataAdapterVM(sqlServerDataAdapterConfig);
            //}
            //else if (value == "SQLite")
            //{
            //    //SelectedAdapterViewModel = new SQLiteDataAdapterVM();
            //    SelectedAdapterViewModel = null;
            //}
            //else if (value == "Excel")
            //{
            //    //SelectedAdapterViewModel = new ExcelDataAdapterVM();
            //    SelectedAdapterViewModel = null;
            //}
            //else
            //{
            //    SelectedAdapterViewModel = null;
            //}
        }
    }
}
