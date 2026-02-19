using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sprout.Core.Views
{
    public partial class MainView : UserControl
    {
        public MainView(MainViewVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                CloseTab(sender);
            }
        }

        private void CloseTab(object sender)
        {
            if (sender is not FrameworkElement control
                || control.DataContext is not ObservableObject tabVM)
            {
                return;
            }

            var viewModel = (MainViewVM)DataContext;
            //the code below fixes binding errors, otherwise the code could be simpler
            MyTabControl.SelectedIndex = -1;

            Dispatcher.BeginInvoke(() =>
            {
                viewModel.Tabs.Remove(tabVM);

                // Update SelectedTab if it was the closed tab
                if (viewModel.SelectedTab == tabVM)
                {
                    viewModel.SelectedTab = viewModel.Tabs.Count > 0 ? viewModel.Tabs[0] : null;
                }
            }, DispatcherPriority.Render);
        }

        private void TabHeaderStackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle)
            {
                CloseTab(sender);
            }
        }
    }
}
