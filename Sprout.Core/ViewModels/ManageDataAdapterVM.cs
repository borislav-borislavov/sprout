using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.Duck;
using Sprout.Core.Services.Dialog;
using System;
using System.Collections.Generic;
using System.Text;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace Sprout.Core.ViewModels
{
    public partial class ManageDataAdapterVM : ObservableObject
    {
        public string[] AdapterTypes { get; set; } = ["SqlServer", "Duck"];

        [ObservableProperty]
        private string _selectedAdapterType;

        [ObservableProperty]
        private IDataAdapterConfigHost _adapterConfigHost;

        [ObservableProperty]
        private ObservableObject _selectedAdapterViewModel;

        private readonly IDialogService _dialogService;

        public ManageDataAdapterVM(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        partial void OnAdapterConfigHostChanged(IDataAdapterConfigHost value)
        {
            if (value.DataAdapter is SqlServerDataAdapterConfig sqlServerDataAdapterConfig)
            {
                SelectedAdapterType = "SqlServer";
                SelectedAdapterViewModel = new SqlServerDataAdapterVM(sqlServerDataAdapterConfig);
            }
            else if (value.DataAdapter is DuckDataAdapterConfig duckDataAdapterConfig)
            {
                SelectedAdapterType = "Duck";
                SelectedAdapterViewModel = new DuckDataAdapterVM(duckDataAdapterConfig);
            }
            else
            {
                SelectedAdapterType = null;
                SelectedAdapterViewModel = null;
            }
        }

        [RelayCommand]
        private void ChangeAdapter()
        {
            try
            {
                if (SelectedAdapterType == "SqlServer")
                {
                    AdapterConfigHost.DataAdapter = new SqlServerDataAdapterConfig
                    {
                        DataProvider = new SqlServerDataProviderConfig
                        {
                            Text = string.Empty
                        },

                        InsertCommand = new SqlServerEditCommandConfig(),
                        UpdateCommand = new SqlServerEditCommandConfig(),
                        DeleteCommand = new SqlServerEditCommandConfig(),
                    };
                    OnAdapterConfigHostChanged(AdapterConfigHost);
                }
                else if (SelectedAdapterType == "Duck")
                {
                    AdapterConfigHost.DataAdapter = new DuckDataAdapterConfig
                    {
                        ConnectionString = "DataSource=:memory:",

                        DataProvider = new DuckDataProviderConfig
                        {
                            Text = string.Empty
                        },

                        InsertCommand = new DuckEditCommandConfig(),
                        UpdateCommand = new DuckEditCommandConfig(),
                        DeleteCommand = new DuckEditCommandConfig(),
                    };

                    OnAdapterConfigHostChanged(AdapterConfigHost);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }
    }
}
