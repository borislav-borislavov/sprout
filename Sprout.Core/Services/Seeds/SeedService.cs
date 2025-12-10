using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.Configurations;
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
        private IMenuService _menuService;

        private SproutConfiguration configuration;

        public SeedService(
            IConfigurationService configurationService,
            IMenuService menuService)
        {
            _configurationService = configurationService;
            _menuService = menuService;
        }

        public void Sprout(Window mainWindow)
        {
            _mainWindow = mainWindow;
            _mainWindow.Content = _menuService.MainView;

            configuration = _configurationService.Load();

            _menuService.CrerateMenu(configuration);
        }
    }
}
