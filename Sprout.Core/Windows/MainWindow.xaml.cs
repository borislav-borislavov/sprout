using Sprout.Core.Views;
using System.ComponentModel;
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

            var currentScreen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            Width = currentScreen.WorkingArea.Width * 0.85;
            Height = currentScreen.WorkingArea.Height * 0.85;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to close Sprout?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            e.Cancel = result != MessageBoxResult.Yes;

            base.OnClosing(e);
        }
    }
}
