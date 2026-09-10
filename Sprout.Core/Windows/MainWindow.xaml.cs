using Sprout.Core.Services.WindowSize;
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
        public static bool ForceClose { get; set; } = false;

        public MainWindow(MainView mainView)
        {
            InitializeComponent();

            Content = mainView;
            var fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly()!.Location).FileVersion;
            Title = string.IsNullOrWhiteSpace(fileVersion) ? "Sprout" : $"Sprout - v{fileVersion}";
                
            WindowScreenSizer.SizeToScreen(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ForceClose)
            {
                base.OnClosing(e);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to close Sprout?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }

            base.OnClosing(e);

            if (!e.Cancel)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
