using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private ObservableCollection<SproutPageVM> _tabs = [];

        [NotifyCanExecuteChangedFor(nameof(EditPageCommand))]
        [ObservableProperty]
        private SproutPageVM _selectedTab;

        public MainViewVM(IConfigurationService configService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _configService = configService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            var sproutConfig = _configService.Load();

            PageConfigs = new ObservableCollection<SproutPageConfiguration>(sproutConfig.Pages);
        }

        [RelayCommand(CanExecute = nameof(CanExecuteOpenPage))]
        private void OpenPage(SproutPageConfiguration pageConfig)
        {
            SelectedTab = new SproutPageVM(pageConfig, _dialogService);
            Tabs.Add(SelectedTab);
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
            var pageTitle = SelectedTab.PageConfig.Title;

            _navigationService.ShowEditPage(SelectedTab.PageConfig, _configService, _dialogService);

            Tabs.Remove(SelectedTab);

            var sproutConfig = _configService.Load();
            PageConfigs = new ObservableCollection<SproutPageConfiguration>(sproutConfig.Pages);

            var currentPageConfig = PageConfigs.FirstOrDefault(pc => pc.Title == pageTitle);

            SelectedTab = new SproutPageVM(currentPageConfig, _dialogService);
            Tabs.Add(SelectedTab);
        }

        private bool CanEditPage() => SelectedTab is not null;

        [RelayCommand]
        private void EditMenu()
        {
            var isSaved = _navigationService.ShowEditMenu(PageConfigs, _configService);

            if (isSaved)
            {
                var sproutConfig = _configService.Load();
                PageConfigs = new ObservableCollection<SproutPageConfiguration>(sproutConfig.Pages);
            }
        }
    }
}
