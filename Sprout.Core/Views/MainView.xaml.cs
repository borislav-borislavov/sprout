using Sprout.Core.ViewModels;
using System.Windows.Controls;

namespace Sprout.Core.Views
{
    public partial class MainView : UserControl
    {
        public MainView(MainViewVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
