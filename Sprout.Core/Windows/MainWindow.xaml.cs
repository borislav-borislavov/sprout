using Sprout.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

            // Calculate 90% of the Primary Screen resolution
            this.Width = SystemParameters.PrimaryScreenWidth * 0.85;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.85;

            // Optional: Center the window on the screen
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
