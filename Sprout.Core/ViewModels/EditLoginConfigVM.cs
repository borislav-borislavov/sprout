using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;

namespace Sprout.Core.ViewModels
{
    public partial class EditLoginConfigVM : ObservableObject
    {
        private readonly IConfigurationService _configService;
        private readonly IDialogService _dialogService;

        public static string[] AdapterTypes { get; } = ["SqlServer", "SQLite"];

        [ObservableProperty]
        private bool _isLoginEnabled;

        [ObservableProperty]
        private string _selectedAdapterType;

        [ObservableProperty]
        private string _updateCommandText;

        public bool IsSaved { get; private set; }

        public EditLoginConfigVM(IConfigurationService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;

            Load();
        }

        private void Load()
        {
            var config = _configService.Load();

            if (config.Login?.DataAdapter is SqlServerDataAdapterConfig sqlAdapter)
            {
                IsLoginEnabled = true;
                SelectedAdapterType = "SqlServer";
                UpdateCommandText = sqlAdapter.Update?.Text ?? string.Empty;
            }
            else if (config.Login?.DataAdapter != null)
            {
                IsLoginEnabled = true;
                SelectedAdapterType = "SQLite";
                UpdateCommandText = string.Empty;
            }
            else
            {
                IsLoginEnabled = false;
                SelectedAdapterType = "SqlServer";
                UpdateCommandText = string.Empty;
            }
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                var config = _configService.Load();

                if (IsLoginEnabled)
                {
                    if (string.IsNullOrWhiteSpace(UpdateCommandText))
                    {
                        _dialogService.ShowMessage("Update Command is required when login is enabled.", "Validation Error", DialogButton.OK, DialogImage.Warning);
                        return;
                    }

                    IDataAdapterConfig adapter;

                    if (SelectedAdapterType == "SqlServer")
                    {
                        var sqlAdapter = config.Login?.DataAdapter as SqlServerDataAdapterConfig
                            ?? new SqlServerDataAdapterConfig
                            {
                                DataProvider = new SqlServerDataProviderConfig()
                            };

                        sqlAdapter.UpdateCommand = new SqlServerEditCommandConfig { Text = UpdateCommandText };
                        adapter = sqlAdapter;
                    }
                    else
                    {
                        throw new NotImplementedException("SQLite adapter is not yet implemented.");
                    }

                    config.Login ??= new LoginConfiguration();
                    config.Login.DataAdapter = adapter;
                }
                else
                {
                    config.Login = null;
                }

                _configService.Save(config);
                IsSaved = true;
                _dialogService.ShowMessage("Login configuration saved.", "Login Configuration", DialogButton.OK, DialogImage.None);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Error", DialogButton.OK, DialogImage.Error);
            }
        }
    }
}
