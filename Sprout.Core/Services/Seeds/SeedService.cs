using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.MainViews;
using Sprout.Core.Services.Menus;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Services.Seeds
{
    public class SeedService : ISeedService
    {
        private Window _mainWindow;

        private IConfigurationService _configurationService;
        private IMainViewService _mainViewService;
        private readonly IMenuService _menuService;
        private SproutConfiguration configuration;

        public SeedService(
            IConfigurationService configurationService,
            IMainViewService mainViewService,
            IMenuService menuService)
        {
            _configurationService = configurationService;
            _mainViewService = mainViewService;
            _menuService = menuService;
        }

        public void Sprout(Window mainWindow)
        {
            configuration = _configurationService.Load();

            _mainWindow = mainWindow;
            _mainWindow.Content = _mainViewService.MainView;
            _menuService.Create(configuration);
        }
    }
}
