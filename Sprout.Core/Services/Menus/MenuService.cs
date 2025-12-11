using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.MainViews;
using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sprout.Core.Services.Menus
{
    public class MenuService : IMenuService
    {
        private readonly IPageFactory _pageFactory;
        private readonly IMainViewService _mainViewService;

        public MenuService(IPageFactory pageFactory, IMainViewService mainViewService)
        {
            _pageFactory = pageFactory;
            _mainViewService = mainViewService;
        }

        public void Create(SproutConfiguration configuration)
        {
            foreach (var page in configuration.Pages)
            {
                var button = new Button();
                button.Content = page.Title;
                button.HorizontalAlignment = HorizontalAlignment.Stretch;
                button.FontSize = 16;
                button.Margin = new Thickness(5, 5, 5, 0);
                button.Command = new OpenPageCommand(_pageFactory, _mainViewService);
                button.CommandParameter = page;

                _mainViewService.MainView.ButtonsContainer.Children.Add(button);
            }
        }
    }

#warning This class could be moved to its own file.
    public class OpenPageCommand : ICommand
    {
        private readonly IPageFactory _pageFactory;
        private readonly IMainViewService _mainViewService;

        public OpenPageCommand(IPageFactory pageFactory, IMainViewService mainViewService)
        {
            _pageFactory = pageFactory;
            _mainViewService = mainViewService;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var pageConfig = (SproutPageConfiguration)parameter!;

            var newTab = new TabItem
            {
                Header = pageConfig.Title,
                Content = _pageFactory.Create(pageConfig)
            };

            _mainViewService.MainView.MyTabControl.Items.Add(newTab);
            _mainViewService.MainView.MyTabControl.SelectedItem = newTab;
        }
    }
}
