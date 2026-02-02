using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.GridActions;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Queries;
using Sprout.Core.UIStates;
using Sprout.Core.Views;
using System.Reflection.Metadata;

namespace Sprout.Core.ViewModels
{
    public partial class SproutPageVM : ObservableObject
    {
        private readonly IDialogService _dialogService;

        public SproutPageConfiguration PageConfig { get; private set; }

        public Dictionary<string, Dictionary<string, GridAction>> GridActions { get; set; } = [];

        public Dictionary<string, IDataAdapter> DataAdapters { get; set; } = [];
        public Dictionary<string, IDataProvider> DataProviders { get; set; } = [];

        public UiStateRegistry UiStateRegistry { get; } = new();

        /// <summary>
        /// Using virtualization to re-create views makes the re-binding the UI State too brittle
        /// and it increases the complexity of the code and the chances for bugs. Code is a liability and this reduces greatly the code complexity.
        /// </summary>
        public SproutPage DynamicViewInstance { get; private set; }

        public SproutPageVM(SproutPageConfiguration pageConfig, IDialogService dialogService)
        {
            PageConfig = pageConfig;
            _dialogService = dialogService;

            try
            {
                CreateDataAdapters();

                UiStateRegistry.UiStateChanged += (_, change) =>
                {
                    foreach (var dataProvider in DataProviders.Values)
                    {
                        var dependencyHasChanged = false;

                        foreach (var dependency in dataProvider.Dependencies)
                        {
                            if (dependency.ControlName == change.ControlName)
                            {
                                dependencyHasChanged = true;

                                var debug = dependency.Value;
                            }
                        }

                        if (dependencyHasChanged)
                        {
                            new DataProviderService().ProvideData(dataProvider);
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

        public void CreateDataAdapters()
        {
            foreach (var kvp in PageConfig.GetDataAdapterConfigs())
            {
                DataAdapters[kvp.Key] = DataAdapterFactory.Create(kvp.Value);

                //done for convenience
                if (DataAdapters[kvp.Key].DataProvider != null)
                {
                    DataProviders[kvp.Key] = DataAdapters[kvp.Key].DataProvider;
                }
            }
        }

        public void OnLoaded()
        {
            try
            {
                var dataProviderService = new DataProviderService();

#warning prevent execution of defined queries that are never used?
                foreach (var kvp in DataProviders)
                {
                    //query dependencies have to be done after all controls are created and before all queries are executed
                    //QueryService.BindDependencies(kvp.Value, UiStateRegistry);
                    dataProviderService.BindDependencies(kvp.Value, UiStateRegistry);

                    dataProviderService.ProvideData(kvp.Value);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Load Error", DialogButton.OK, DialogImage.Error);
            }
        }

        [RelayCommand]
        private void PerformAction(object parameter)
        {
			try
			{
				if (parameter is GridAction gridAction)
				{
                    gridAction.Perform(DataAdapters, UiStateRegistry);
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
    }
}
