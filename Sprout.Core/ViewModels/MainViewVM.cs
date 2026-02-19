using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Messages;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Navigation;
using System.Collections.ObjectModel;

namespace Sprout.Core.ViewModels
{
    public partial class MainViewVM : ObservableObject
    {
        private readonly IConfigurationService _configService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        [ObservableProperty]
        private ObservableCollection<SproutPageConfiguration> _pageConfigs;

        [ObservableProperty]

        private ObservableCollection<ObservableObject> _tabs = [];

        [NotifyCanExecuteChangedFor(nameof(EditPageCommand))]
        [ObservableProperty]
        private ObservableObject _selectedTab;

        private SproutConfiguration _sproutConfig;

        public MainViewVM(IConfigurationService configService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _configService = configService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            LoadMenuPages();

            WeakReferenceMessenger.Default.Register<OpenTabMessage>(this, (r, msg) =>
            {
                var pageConfig = _sproutConfig.Pages.FirstOrDefault(p => p.ID == msg.Value.PageConfigID);

                if (pageConfig != null)
                {
                    OpenTab(pageConfig, msg.Value.Parameter);
                }
                else
                {
                    _dialogService.ShowMessage($"Page with ID {msg.Value.PageConfigID} not found.", "Error");
                }
            });
        }

        private void LoadMenuPages()
        {
            _sproutConfig = _configService.Load();

            var visiblePages = _sproutConfig.Pages.Where(p => p.AddToMenu);
            PageConfigs = new ObservableCollection<SproutPageConfiguration>(visiblePages);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteOpenPage))]
        private void OpenPage(SproutPageConfiguration pageConfig)
        {
#warning rename this to OpenTab as well for consistency
            OpenTab(pageConfig, null);
        }

        private void OpenTab(SproutPageConfiguration pageConfig, object? parameter)
        {
            var tab = new SproutPageVM(pageConfig, _dialogService);

            if (parameter != null)
            {
                tab.SproutPageUIState.Data = parameter;
            }

            SelectedTab = tab;
            Tabs.Add(tab);
        }

        [RelayCommand]
        private void OpenSettings()
        {
            var existing = Tabs.OfType<SettingsVM>().FirstOrDefault();
            if (existing != null)
            {
                SelectedTab = existing;
                return;
            }

            var settingsVM = new SettingsVM(_configService, _dialogService);
            Tabs.Add(settingsVM);
            SelectedTab = settingsVM;
        }

        private bool CanExecuteOpenPage(SproutPageConfiguration pageConfig)
        {
#warning Make a setting on the page to allow multiple tabs of the same page.
            return true;
            //return Tabs.Any(t => t.PageConfig == pageConfig) == false;
        }

        [RelayCommand(CanExecute = nameof(CanEditPage))]
        private void EditPage()
        {
            if (SelectedTab is not SproutPageVM selectedPageVM) return;

            var pageId = selectedPageVM.PageConfig.ID;

            _navigationService.ShowEditPage(selectedPageVM.PageConfig, _configService, _dialogService);

            var uiState = selectedPageVM.SproutPageUIState;

            Tabs.Remove(selectedPageVM);

            LoadMenuPages();

            var currentPageConfig = _sproutConfig.Pages.FirstOrDefault(pc => pc.ID == pageId);

            var newTab = new SproutPageVM(currentPageConfig, _dialogService);
            newTab.SproutPageUIState.Data = uiState.Data;
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private bool CanEditPage() => SelectedTab is SproutPageVM;

        [RelayCommand]
        private void EditMenu()
        {
            var isSaved = _navigationService.ShowEditMenu(PageConfigs, _configService);

            if (isSaved)
            {
                LoadMenuPages();
            }
        }
    }
}
