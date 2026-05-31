using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    public partial class EditMenuVM : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<SproutPageConfiguration> _pageConfigs = [];

        [ObservableProperty]
        private SproutPageConfiguration _selectedPageConfig;

        [ObservableProperty]
        private ObservableCollection<MenuCategoryVM> _categories = [];

        [ObservableProperty]
        private MenuCategoryVM _selectedCategory;

        private readonly IConfigurationService _configurationService;

        private SproutConfiguration _sproutConfig;

        public bool IsSaved { get; internal set; }

        public EditMenuVM(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            //await Task.Delay(2000);

            _sproutConfig = _configurationService.Load();

            PageConfigs.Clear();

            foreach (var pageConfig in _sproutConfig.Pages)
            {
                PageConfigs.Add(pageConfig);
            }

            Categories.Clear();

            foreach (var category in _sproutConfig.Categories)
            {
                var categoryVM = new MenuCategoryVM
                {
                    ID = category.ID,
                    Title = category.Title,
                    BackgroundColor = category.BackgroundColor
                };

                foreach (var page in PageConfigs.Where(p => p.CategoryID == category.ID))
                {
                    categoryVM.Pages.Add(page);
                }

                Categories.Add(categoryVM);
            }

            //return Task.CompletedTask;
        }

        [RelayCommand]
        private void CreateNew()
        {
            var pageConfig = new SproutPageConfiguration
            {
                Title = "New Page",
                ID = Guid.NewGuid(),
                AddToMenu = true,
                Root = new GridConfig()
                {
                    Name = "RootGrid",
                    Rows = ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],
                    Columns= ["*", "*", "*", "*", "*", "*", "*", "*", "*", "*"],

                }
            };

            PageConfigs.Add(pageConfig);

            SelectedPageConfig = pageConfig;
        }

        [RelayCommand]
        private void CreateNewCategory()
        {
            var category = new MenuCategoryVM
            {
                ID = Guid.NewGuid(),
                Title = "New Category"
            };

            Categories.Add(category);

            SelectedCategory = category;
        }

        [RelayCommand]
        private void DeleteSelectedCategory()
        {
            if (SelectedCategory == null) return;

            foreach (var page in SelectedCategory.Pages)
            {
                page.CategoryID = null;
            }

            Categories.Remove(SelectedCategory);
        }

        [RelayCommand]
        private void AssignSelectedPageToCategory()
        {
            if (SelectedPageConfig == null || SelectedCategory == null) return;

            foreach (var category in Categories)
            {
                if (category.Pages.Contains(SelectedPageConfig))
                {
                    category.Pages.Remove(SelectedPageConfig);
                }
            }

            SelectedPageConfig.CategoryID = SelectedCategory.ID;

            if (!SelectedCategory.Pages.Contains(SelectedPageConfig))
            {
                SelectedCategory.Pages.Add(SelectedPageConfig);
            }
        }

        [RelayCommand]
        private void RemoveSelectedPageFromCategory()
        {
            if (SelectedPageConfig == null) return;

            foreach (var category in Categories)
            {
                category.Pages.Remove(SelectedPageConfig);
            }

            SelectedPageConfig.CategoryID = null;
        }

        [RelayCommand]
        private void MoveCategoryUp()
        {
            if (SelectedCategory == null) return;
            int index = Categories.IndexOf(SelectedCategory);
            if (index > 0)
            {
                Categories.Move(index, index - 1);
            }
        }

        [RelayCommand]
        private void MoveCategoryDown()
        {
            if (SelectedCategory == null) return;
            int index = Categories.IndexOf(SelectedCategory);
            if (index >= 0 && index < Categories.Count - 1)
            {
                Categories.Move(index, index + 1);
            }
        }

        [RelayCommand]
        private void Save()
        {
            _sproutConfig.Pages = PageConfigs.ToList();

            _sproutConfig.Categories = Categories
                .Select(c => new SproutMenuCategory
                {
                    ID = c.ID,
                    Title = c.Title,
                    BackgroundColor = c.BackgroundColor
                })
                .ToList();

            _configurationService.Save(_sproutConfig);

            IsSaved = true;
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            if (SelectedPageConfig != null)
            {
                foreach (var category in Categories)
                {
                    category.Pages.Remove(SelectedPageConfig);
                }

                PageConfigs.Remove(SelectedPageConfig);
            }
        }

        [RelayCommand]
        private void MoveUp()
        {
            if (SelectedPageConfig == null) return;
            int index = PageConfigs.IndexOf(SelectedPageConfig);
            if (index > 0)
            {
                PageConfigs.Move(index, index - 1);
            }
        }

        [RelayCommand]
        private void MoveDown()
        {
            if (SelectedPageConfig == null) return;
            int index = PageConfigs.IndexOf(SelectedPageConfig);
            if (index >= 0 && index < PageConfigs.Count - 1)
            {
                PageConfigs.Move(index, index + 1);
            }
        }
    }
}
