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
        private void Save()
        {
            _sproutConfig.Pages = PageConfigs.ToList();

            _configurationService.Save(_sproutConfig);

            IsSaved = true;
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            if (SelectedPageConfig != null)
            {
                PageConfigs.Remove(SelectedPageConfig);
            }
        }
    }
}
