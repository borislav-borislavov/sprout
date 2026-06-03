using Sprout.Core.ViewModels;
using System.Windows;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for IconBrowserWindow.xaml
    /// </summary>
    public partial class IconBrowserWindow : Window
    {
        public IconBrowserWindow(IconBrowserVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
