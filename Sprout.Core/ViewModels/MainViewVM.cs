using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Factories;
using Sprout.Core.Messages;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.ActionMessageService;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.CPL;
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
        private readonly ISproutPageVMFactory _sproutPageVMFactory;
        private readonly IVMFactory _vmFactory;

        [ObservableProperty]
        private ObservableCollection<SproutPageConfiguration> _pageConfigs;

        [ObservableProperty]
        private ObservableCollection<MenuCategoryVM> _categories = [];

        [ObservableProperty]

        private ObservableCollection<ObservableObject> _tabs = [];

        [NotifyCanExecuteChangedFor(nameof(EditPageCommand))]
        [ObservableProperty]
        private ObservableObject _selectedTab;

        private SproutConfiguration _sproutConfig;

        public MainViewVM(IConfigurationService configService,
            INavigationService navigationService,
            IDialogService dialogService,
            ISproutPageVMFactory sproutPageVMFactory,
            IVMFactory vmFactory)
        {
            _configService = configService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _sproutPageVMFactory = sproutPageVMFactory;
            _vmFactory = vmFactory;
            LoadMenuPages();

            WeakReferenceMessenger.Default.Register<OpenTabMessage>(this, (r, msg) =>
            {
                var pageConfig = _sproutConfig.Pages.FirstOrDefault(p => p.ID == msg.Value.PageConfigID);

                if (pageConfig != null)
                {
                    OpenTab(pageConfig, msg.Value);
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

            var visiblePages = _sproutConfig.Pages.Where(p => p.AddToMenu).ToList();
            PageConfigs = new ObservableCollection<SproutPageConfiguration>(
                visiblePages.Where(p => p.CategoryID == null));

            Categories.Clear();

            foreach (var category in _sproutConfig.Categories)
            {
                var categoryVM = new MenuCategoryVM
                {
                    ID = category.ID,
                    Title = category.Title,
                    BackgroundColor = category.BackgroundColor
                };

                foreach (var page in visiblePages.Where(p => p.CategoryID == category.ID))
                {
                    categoryVM.Pages.Add(page);
                }

                Categories.Add(categoryVM);
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteOpenPage))]
        private void OpenPage(SproutPageConfiguration pageConfig)
        {
#warning rename this to OpenTab as well for consistency
            OpenTab(pageConfig, null);
        }

        private void OpenTab(SproutPageConfiguration pageConfig, OpenTabMessageArgs? args)
        {
            var sproutPageVM = _sproutPageVMFactory.Create(pageConfig, args);

            SelectedTab = sproutPageVM;
            Tabs.Add(sproutPageVM);
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

        [RelayCommand]
        private void OpenMigrations()
        {
            var existing = Tabs.OfType<MigrationVM>().FirstOrDefault();
            if (existing != null)
            {
                SelectedTab = existing;
                return;
            }

            var migrationVM = _vmFactory.Create<MigrationVM>();

            Tabs.Add(migrationVM);
            SelectedTab = migrationVM;
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

            if (currentPageConfig == null)
            {
                _dialogService.ShowError("Unable to find requested page");
                return;
            }

            var newTab = _sproutPageVMFactory.Create(currentPageConfig, new OpenTabMessageArgs
            {
                PageConfigID = currentPageConfig.ID,
                Parameter = uiState.Data
            });

            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        private bool CanEditPage() => SelectedTab is SproutPageVM;

        [RelayCommand]
        private void EditMenu()
        {
            var isSaved = _navigationService.ShowEditMenu();

            if (isSaved)
            {
                LoadMenuPages();
            }
        }

        [RelayCommand]
        private void EditLoginConfig()
        {
            var isSaved = _navigationService.ShowEditLoginConfig();

            if (isSaved)
            {
                LoadMenuPages();
            }
        }

        private bool CanEditPageScript() => SelectedTab is SproutPageVM;

        [RelayCommand(CanExecute = nameof(CanEditPageScript))]
        private void EditPageScript()
        {
            try
            {
                if (SelectedTab is not SproutPageVM selectedPageVM) return;

                var compiler = new CustomPageLogicCompiler(selectedPageVM, _configService);

                var pageId = selectedPageVM.PageConfig.ID;

                _navigationService.ShowScriptEditor(compiler);

                var uiState = selectedPageVM.SproutPageUIState; //the data that is passed by another page

                Tabs.Remove(selectedPageVM);

                //this just refreshes the _sproutConfig
                LoadMenuPages();

                var currentPageConfig = _sproutConfig.Pages.FirstOrDefault(pc => pc.ID == pageId);

                if (currentPageConfig == null)
                {
                    _dialogService.ShowError("Unable to find requested page, all changes are lost");
                    return;
                }

                var newTab = _sproutPageVMFactory.Create(currentPageConfig, new OpenTabMessageArgs
                {
                    PageConfigID = currentPageConfig.ID,
                    Parameter = uiState.Data
                });

                Tabs.Add(newTab);
                SelectedTab = newTab;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error editing page script: {ex.Message}");
            }
        }
    }
}
