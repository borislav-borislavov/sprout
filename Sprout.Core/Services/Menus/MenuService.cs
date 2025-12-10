using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.PageUIs;
using Sprout.Core.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sprout.Core.Services.Menus
{
    public class MenuService : IMenuService
    {
        private readonly IPageUIService _pageUIService;
        private readonly IPageFactory pageFactory;

        public MainView MainView { get; private set; }

        public MenuService(IPageUIService pageUIService, IPageFactory pageFactory)
        {
            _pageUIService = pageUIService;
            this.pageFactory = pageFactory;
            MainView = new MainView();
        }

        public void CrerateMenu(SproutConfiguration configuration)
        {
            foreach (var page in configuration.Pages)
            {
                var button = new Button();
                button.Content = page.Title;
                button.HorizontalAlignment = HorizontalAlignment.Stretch;
                button.FontSize = 16;
                button.Margin = new Thickness(5, 5, 5, 0);
                button.Command = new OpenPageCommand(pageFactory, page, MainView);
                button.CommandParameter =

                MainView.ButtonsContainer.Children.Add(button);
            }
        }
    }

#warning This class could be moved to its own file.
    class OpenPageCommand : ICommand
    {
        private readonly IPageFactory _pageFactory;
        private SproutPageConfiguration _pageConfig;
        private readonly MainView mainView;

        public OpenPageCommand(IPageFactory pageFactory, SproutPageConfiguration pageConfig, MainView mainView)
        {
            _pageFactory = pageFactory;
            _pageConfig = pageConfig;
            this.mainView = mainView;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var newTab = new TabItem
            {
                Header = _pageConfig.Title,
                Content = _pageFactory.Create(_pageConfig)
            };

            mainView.MyTabControl.Items.Add(newTab);
            mainView.MyTabControl.SelectedItem = newTab;
        }
    }
}
