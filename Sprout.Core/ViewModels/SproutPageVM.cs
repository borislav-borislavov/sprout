using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Factories;
using Sprout.Core.Messages;
using Sprout.Core.Models;
using Sprout.Core.Models.ButtonActions;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using Sprout.Core.Views;
using System.Reflection.Metadata;
using System.Windows;

namespace Sprout.Core.ViewModels
{
    public partial class SproutPageVM : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IDataAdapterFactory _dataAdapterFactory;
        private readonly IDataServiceFactory _dataServiceFactory;

        public SproutPageConfiguration PageConfig { get; private set; }

        public string Title => PageConfig?.Title;

        /// <summary>
        /// The starting args that a page receives when started as a child page
        /// </summary>
        public SproutPageUIState SproutPageUIState { get; } = new();

        public Dictionary<string, Dictionary<string, IButtonAction>> ButtonActions { get; set; } = [];

        public Dictionary<string, IDataAdapter> DataAdapters { get; set; } = [];
        public Dictionary<string, IDataProvider> DataProviders { get; set; } = [];

        public UiStateRegistry UiStateRegistry { get; } = new();

        /// <summary>
        /// Using virtualization to re-create views makes the re-binding the UI State too brittle
        /// and it increases the complexity of the code and the chances for bugs. Code is a liability and this reduces greatly the code complexity.
        /// </summary>
        public SproutPage DynamicViewInstance { get; private set; }

        public SproutPageVM(SproutPageConfiguration pageConfig,
            IDialogService dialogService,
            IDataAdapterFactory dataAdapterFactory,
            IDataServiceFactory dataServiceFactory)
        {
            PageConfig = pageConfig;
            _dialogService = dialogService;
            _dataAdapterFactory = dataAdapterFactory;
            _dataServiceFactory = dataServiceFactory;

            try
            {
                CreateDataAdapters();

                UiStateRegistry.UiStateChanged += async (_, change) =>
                {
#warning add try catch here!!!
                    foreach (var dataProvider in DataProviders.Values)
                    {
                        var dependencyHasChanged = false;

                        foreach (var dependency in dataProvider.Dependencies)
                        {
                            if (dependency.ControlName == change.ControlName)
                            {
                                dependencyHasChanged = true;
                            }
                        }

                        if (dependencyHasChanged)
                        {
                            using (var dataService = _dataServiceFactory.Create(dataProvider.Parent, UiStateRegistry))
                            {
                                await dataService.ProvideData();
                            }
                        }
                    }
                };

                DynamicViewInstance = new SproutPage { DataContext = this };
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Ctor Error", DialogButton.OK, DialogImage.Error);
            }
        }

        public void RegisterOwnUIState()
        {
            UiStateRegistry.Register("Page", SproutPageUIState);
        }

        public void CreateDataAdapters()
        {
            foreach (var kvp in PageConfig.GetDataAdapterConfigs())
            {
                DataAdapters[kvp.Key] = _dataAdapterFactory.Create(kvp.Value);

                //done for convenience
                if (DataAdapters[kvp.Key].DataProvider != null)
                {
                    DataProviders[kvp.Key] = DataAdapters[kvp.Key].DataProvider;
                }
            }
        }

        public async void OnLoaded()
        {
            try
            {
                foreach (var kvp in DataProviders)
                {
                    DependencyBinder.BindDependencies(kvp.Value, UiStateRegistry);
                }

                foreach (var kvp in DataProviders)
                {
                    using (var dataservice = _dataServiceFactory.Create(kvp.Value.Parent, UiStateRegistry))
                    {
                        await dataservice.ProvideData();
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
                    await buttonAction.Perform(DataAdapters, UiStateRegistry, _dataServiceFactory);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Action Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void Filter()
        {
            try
            {

            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Filter Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void DisplayItemPage(object parameter)
        {
            if (parameter is not ItemDisplayPageInfo itemDisplayInfo)
                return;

            var sproutGridUiState = UiStateRegistry.Get<SproutGridUIState>(itemDisplayInfo.GridName);

            if (sproutGridUiState == null)
            {
                //TODO
                return;
            }

            if (sproutGridUiState.Selected == null)
            {
                //TODO
                return;
            }

            var args = new OpenTabMessageArgs()
            {
                PageConfigID = itemDisplayInfo.ItemDisplayPageID,
                Parameter = sproutGridUiState.Selected
            };

            WeakReferenceMessenger.Default.Send(new OpenTabMessage(args));
        }
    }
}
