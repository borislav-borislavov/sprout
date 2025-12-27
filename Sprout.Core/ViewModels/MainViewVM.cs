using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    public partial class MainViewVM : ObservableObject
    {
        private readonly IConfigurationService _configService;

        [ObservableProperty]
        private ObservableCollection<SproutPageConfiguration> _pageConfigs;

        [ObservableProperty]
        private ObservableCollection<SproutPageVM> _tabs = [];

        [ObservableProperty]
        private SproutPageVM _selectedTab;

        public MainViewVM(IConfigurationService configService)
        {
            _configService = configService;

            var sproutConfig = _configService.Load();

            PageConfigs = new ObservableCollection<SproutPageConfiguration>(sproutConfig.Pages);
        }

        [RelayCommand()]
        private void OpenPage(SproutPageConfiguration pageConfig)
        {
            SelectedTab = new SproutPageVM(pageConfig);
            Tabs.Add(SelectedTab);
        }
    }
}
