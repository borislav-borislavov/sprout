using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using System.Xml;

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

        public TextDocument UpdateDocument { get; } = new TextDocument();
        public IHighlightingDefinition Highlighting { get; private set; }

        public bool IsSaved { get; private set; }

        public EditLoginConfigVM(IConfigurationService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;

            using var stream = GetType().Assembly
                .GetManifestResourceStream("Sprout.Core.TSQL.xshd");
            using var reader = XmlReader.Create(stream!);
            Highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

            Load();
        }

        private void Load()
        {
            var config = _configService.Load();

            if (config.Login?.DataAdapter is SqlServerDataAdapterConfig sqlAdapter)
            {
                IsLoginEnabled = config.Login.IsEnabled;
                SelectedAdapterType = "SqlServer";
                UpdateDocument.Text = sqlAdapter.Update?.Text ?? string.Empty;
            }
            else if (config.Login?.DataAdapter != null)
            {
                IsLoginEnabled = config.Login.IsEnabled;
                SelectedAdapterType = "SQLite";
                UpdateDocument.Text = string.Empty;
            }
            else
            {
                IsLoginEnabled = false;
                SelectedAdapterType = "SqlServer";
                UpdateDocument.Text = string.Empty;
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
                    if (string.IsNullOrWhiteSpace(UpdateDocument.Text))
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

                        sqlAdapter.UpdateCommand = new SqlServerEditCommandConfig { Text = UpdateDocument.Text };
                        adapter = sqlAdapter;
                    }
                    else
                    {
                        throw new NotImplementedException("SQLite adapter is not yet implemented.");
                    }

                    config.Login ??= new LoginConfiguration();
                        config.Login.IsEnabled = true;
                        config.Login.DataAdapter = adapter;
                    }
                    else
                    {
                        if (config.Login != null)
                            config.Login.IsEnabled = false;
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
