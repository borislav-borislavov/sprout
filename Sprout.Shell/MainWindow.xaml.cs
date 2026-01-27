using Sprout.Core.Views;
using System.Windows;

namespace Sprout.Shell
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainView mainView)
        {
            InitializeComponent();

            Content = mainView;

            // Calculate 90% of the Primary Screen resolution
            this.Width = SystemParameters.PrimaryScreenWidth * 0.9;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.9;

            // Optional: Center the window on the screen
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}