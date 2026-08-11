using Sprout.Core.Views;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainView mainView)
        {
            InitializeComponent();

            Content = mainView;
            var fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly()!.Location).FileVersion;
            Title = string.IsNullOrWhiteSpace(fileVersion) ? "Sprout" : $"Sprout - v{fileVersion}";

            // Calculate 85% of the Primary Screen resolution
            this.Width = SystemParameters.PrimaryScreenWidth * 0.85;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.85;

            // Optional: Center the window on the screen
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
