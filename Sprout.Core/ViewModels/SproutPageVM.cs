using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Messages;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Services;
using Sprout.Core.Services.ActionMessageService;
using Sprout.Core.Services.Clipboard;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Login;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.Views;
using System.Diagnostics;

namespace Sprout.Core.ViewModels
{
    public partial class SproutPageVM : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IActionMessageService _actionMessageService;
        private readonly IDataAdapterFactory _dataAdapterFactory;
        private readonly IDataServiceFactory _dataServiceFactory;
        private readonly ILoggedInUserService _loggedInUserService;
        private readonly IConfigurationService _configurationService;
        private readonly ISproutControlFactory _sproutControlFactory;

        public SproutPageConfiguration PageConfig { get; private set; }

        private string _customTitle;

        public string Title => _customTitle ?? PageConfig?.Title;

        public void RenameTab(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                return;
            }

            _customTitle = newTitle;
            OnPropertyChanged(nameof(Title));
        }

        /// <summary>
        /// The starting args that a page receives when started as a child page
        /// </summary>
        public SproutPageInternalVM SproutPageInternalVM { get; } = new();

        public VMRegistry VMRegistry { get; } = new();

        /// <summary>
        /// Using virtualization to re-create views makes the re-binding the VM too brittle
        /// and it increases the complexity of the code and the chances for bugs. Code is a liability and this reduces greatly the code complexity.
        /// </summary>
        public SproutPage DynamicViewInstance { get; private set; }
        public CompileResult CompileResult { get; set; }

        public Sprout.Core.Services.Clipboard.IClipboardService ClipboardService { get; }

        private SproutControlVMs.LoginVM _loginUIState = new();

        private readonly SproutPageLogicBridge _host;

        public SproutPageVM(SproutPageConfiguration pageConfig,
            OpenTabMessageArgs? args,
            IDialogService dialogService,
            IActionMessageService actionMessageService,
            IDataAdapterFactory dataAdapterFactory,
            IDataServiceFactory dataServiceFactory,
            ILoggedInUserService loggedInUserService,
            IConfigurationService configurationService,
            IClipboardService clipboardService,
            ISproutControlFactory sproutControlFactory)
        {
            PageConfig = pageConfig;
            SproutPageInternalVM.Data = args?.Parameter;
            _dialogService = dialogService;
            _actionMessageService = actionMessageService;
            _dataAdapterFactory = dataAdapterFactory;
            _dataServiceFactory = dataServiceFactory;
            _loggedInUserService = loggedInUserService;
            _configurationService = configurationService;
            ClipboardService = clipboardService;
            _sproutControlFactory = sproutControlFactory;

            try
            {
                DynamicViewInstance = new SproutPage(_configurationService, _sproutControlFactory) { DataContext = this };
                DynamicViewInstance.InitializeControls(this);

                foreach (var controlVM in VMRegistry.ViewModels.Values)
                {
                    controlVM.OwnerPageID = PageConfig.ID;
                }

                _host = new SproutPageLogicBridge($"{PageConfig.ID.ToString().Replace("-", "")}");

                VMRegistry.VMChanged += async (_, change) =>
                {
                    if (change.PropertyName == "IsBusy")
                    {
                        return;
                    }

                    foreach (var kvp in VMRegistry.ViewModels)
                    {
                        if (kvp.Value is IDependent dependent)
                        {
                            foreach (var dependency in dependent.Dependencies)
                            {
                                if (dependency.ControlName == change.ControlName)
                                {
                                    dependent.DepenencyChanged(dependency, VMRegistry);
                                }
                            }
                        }

                        if (kvp.Value is IDataAdapterHost dataAdapterHost && dataAdapterHost.DataAdapter != null)
                        {
                            var dependencyHasChanged = false;

                            foreach (var dependency in dataAdapterHost.DataAdapter.DataProvider.Dependencies)
                            {
                                if (dependency.ControlName == change.ControlName)
                                {
                                    dependencyHasChanged = true;
                                }
                            }

                            if (dependencyHasChanged)
                            {
                                try
                                {
                                    using var dataService = _dataServiceFactory.Create(dataAdapterHost.DataAdapter, VMRegistry);
                                    await dataService.ProvideData();
                                }
                                catch (Exception ex)
                                {
                                    _dialogService.ShowMessage(ex.Message, "Dependency changed Error", DialogButton.OK, DialogImage.Error);
                                }
                            }
                        }

                        if (kvp.Value is IDataAdapterDictionaryHost dataAdapterDictionary)
                        {
                            foreach (var dataAdapter in dataAdapterDictionary.DataAdapters)
                            {
                                var dependencyHasChanged = false;
                                foreach (var dependency in dataAdapter.Value.DataProvider.Dependencies)
                                {
                                    if (dependency.ControlName == change.ControlName)
                                    {
                                        dependencyHasChanged = true;
                                    }
                                }

                                if (dependencyHasChanged)
                                {
                                    try
                                    {
                                        using var dataService = _dataServiceFactory.Create(dataAdapter.Value, VMRegistry);
                                        await dataService.ProvideData();
                                    }
                                    catch (Exception ex)
                                    {
                                        _dialogService.ShowMessage(ex.Message, "Dependency changed Error", DialogButton.OK, DialogImage.Error);
                                    }
                                }
                            }
                        }
                    }
                };

                DynamicViewInstance.InitializePage(this);
                OnPageInitialize();

                if (_loggedInUserService?.UserDataAdapter?.DataProvider?.Data is System.Data.DataTable loginUserDt && loginUserDt.Rows.Count > 0)
                {
                    _loginUIState.User = loginUserDt.DefaultView[0];
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Ctor Error", DialogButton.OK, DialogImage.Error);
            }
        }

        /// <summary>
        /// Load the custom page logic
        /// </summary>
        private void LoadCPL()
        {
            try
            {
                if (CompileResult == null) return;

                if (!CompileResult.IsSuccess)
                {
                    //return CompileResult.Diagnostics; // Surface errors in your code editor UI
                    _dialogService.ShowError(string.Join("\n", CompileResult.Diagnostics.Select(d => $"{d.Severity}: {d.Message} at {d.Line}:{d.Column}")));
                    return;
                }

                if (CompileResult.LiveDebugPage != null)
                {
                    string? error = _host.LoadLiveDebug(CompileResult.LiveDebugPage, pageContext: this).Result;
                    if (error is not null)
                        _dialogService.ShowError(error);
                }
                else
                {
                    string? error = _host.LoadAsync(CompileResult.Assembly!, pageContext: this).Result;
                    if (error is not null)
                        _dialogService.ShowError(error);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Load CPL Error", DialogButton.OK, DialogImage.Error);
            }
        }

        public void RegisterExtraVMs()
        {
            VMRegistry.Register(SproutPageInternalVM);
            VMRegistry.Register(_loginUIState);
        }

        public async void OnPageInitialize()
        {
            try
            {
                LoadCPL();

                /*new logic for binding data adapter dependencies*/
                foreach ((string vmKey, BaseSproutControlVM vm) in VMRegistry.ViewModels)
                {
                    if (vm is SproutButtonVM)
                    {
                        //The DataProvider dependencies of buttons are intentionally skipped to provide a more intuitive behavior of the control.
                        //Their values are set manually before the button action executes ProvideData.
                        continue;
                    }

                    if (vm is IDataAdapterHost dataAdapterHost)
                    {
                        DependencyBinder.BindDependencies(dataAdapterHost.DataAdapter.DataProvider, VMRegistry);
                    }

                    if (vm is IDataAdapterDictionaryHost adapterDictionaryHost)
                    {
                        foreach ((string adapterKey, IDataAdapter dataAdapter) in adapterDictionaryHost.DataAdapters)
                        {
                            DependencyBinder.BindDependencies(dataAdapter.DataProvider, VMRegistry);
                        }
                    }

                }

                /*New logic for providing data*/
                foreach ((string key, BaseSproutControlVM viewModel) in VMRegistry.ViewModels)
                {
                    if (viewModel is IDataAdapterDictionaryHost dataAdapterDictionaryHost)
                    {
                        foreach ((_, IDataAdapter dataAdapter) in dataAdapterDictionaryHost.DataAdapters)
                        {
                            var dataProvider = dataAdapter.DataProvider;

                            if (dataProvider.DeferInitialLoad)
                                continue;

                            if (SproutPageInternalVM.Data == null && //detail pages should load initially if they have data
                                dataProvider.Dependencies.Count() > 0) //experimental optimization to not run queries which depend on other values for nothing
                            {
                                continue;
                            }

                            using var dataservice = _dataServiceFactory.Create(dataProvider.Parent, VMRegistry);
                            await dataservice.ProvideData();
                        }
                    }

                    if (viewModel is IDataAdapterHost dataAdapterHost && dataAdapterHost.DataAdapter != null)
                    {
                        var dataProvider = dataAdapterHost.DataAdapter.DataProvider;

                        var loadData = true;

                        if (dataProvider.DeferInitialLoad)
                            loadData = false;

                        if (SproutPageInternalVM.Data == null && //detail pages should load initially if they have data
                            dataProvider.Dependencies.Count() > 0) //experimental optimization to not run queries which depend on other values for nothing
                        {
                            loadData = false;
                        }

                        if (loadData)
                        {
                            using var dataservice = _dataServiceFactory.Create(dataProvider.Parent, VMRegistry);
                            await dataservice.ProvideData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Load Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private async Task PerformAction(object parameter)
        {
            try
            {
                if (parameter is IButtonAction buttonAction)
                {
                    await buttonAction.Perform(VMRegistry, _dataServiceFactory);

                    if (parameter is IButtonActionMessenger messenge)
                        _actionMessageService.Show(messenge);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Action Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void DisplayListItemPage(object parameter)
        {
            if (parameter is not ListPageLaunchInfo launchInfo)
                return;

            var sproutListUiState = VMRegistry.Get<SproutListVM>(launchInfo.ListName);

            if (sproutListUiState == null)
            {
                return;
            }

            if (sproutListUiState.Selected == null)
            {
                _dialogService.ShowMessage("Please select an item from the list first.", "No selection");
                return;
            }

            var args = new OpenTabMessageArgs()
            {
                PageConfigID = launchInfo.PageId,
                Parameter = sproutListUiState.Selected
            };

            WeakReferenceMessenger.Default.Send(new OpenTabMessage(args));
        }
    }
}
