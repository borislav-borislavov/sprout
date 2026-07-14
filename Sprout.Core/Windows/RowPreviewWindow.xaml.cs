using Sprout.Core.ViewModels;
using System.Windows;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for RowPreviewWindow.xaml
    /// </summary>
    public partial class RowPreviewWindow : Window
    {
        public RowPreviewVM ViewModel { get; private set; }

        public RowPreviewWindow(RowPreviewVM vm)
        {
            InitializeComponent();
            DataContext = vm;
            ViewModel = vm;
        }
    }
}
