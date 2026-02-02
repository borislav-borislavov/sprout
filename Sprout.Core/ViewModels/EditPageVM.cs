using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Common;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Navigation;
using Sprout.Core.Views.Controls;
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
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public string[] AdapterTypes { get; set; } = ["SqlServer", "SQLite", "Excel"];

        [ObservableProperty]
        private string _selectedAdapterType;

        [ObservableProperty]
        private ObservableObject _selectedAdapterViewModel;

        [ObservableProperty]
        private bool _areFiltersVisible;

        public EditPageVM(IConfigurationService configService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _configService = configService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        public void Initialize(SproutPageConfiguration pageConfig)
        {
            PageConfig = pageConfig.Clone();

            if (PageConfig.Root is not GridConfig gridConfig)
            {
                return;
            }

            Controls.Add(gridConfig);
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                var debug = PageConfig;

                var sproutConfig = _configService.Load();
                var foundPage = sproutConfig.Pages.FirstOrDefault(p => p.Title == PageConfig.Title);
                var pageIndex = sproutConfig.Pages.IndexOf(foundPage);
                sproutConfig.Pages.Remove(foundPage);
                sproutConfig.Pages.Insert(pageIndex, PageConfig);

                _configService.Save(sproutConfig);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void AddControl()
        {
            try
            {
                SproutControlConfig newControl = _navigationService.ShowAddControl();

                if (newControl == null) return;

#warning create interface to mark containers
                if (SelectedNode is GridConfig gridConfig)
                {
                    gridConfig.Children.Add(newControl);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }

        partial void OnSelectedNodeChanged(SproutControlConfig value)
        {
            try
            {
                if (value is SproutDataGridConfig dataGridConfig && dataGridConfig.DataAdapter is SqlServerDataAdapterConfig)
                {
                    AreFiltersVisible = true;
                }
                else
                {
                    AreFiltersVisible = false;
                }

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
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void ChangeAdapter()
        {
            try
            {
                if (SelectedNode is not IDataAdapterControlConfig adapterControl) return;

                if (SelectedAdapterType == "SqlServer")
                {
                    adapterControl.DataAdapter = new SqlServerDataAdapterConfig
                    {
                        ConnectionString = "Server=.;Database=DbName;Trusted_Connection=True;TrustServerCertificate=Yes",

                        DataProvider = new SqlServerDataProviderConfig
                        {
                            Text = string.Empty
                        },

                        InsertCommand = new SqlServerEditCommandConfig(),
                        UpdateCommand = new SqlServerEditCommandConfig(),
                        DeleteCommand = new SqlServerEditCommandConfig(),
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }
    }
}
