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
        }
    }
}