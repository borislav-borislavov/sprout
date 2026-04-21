using Sprout.Core.ViewModels;
using System.Windows;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for EditLoginConfig.xaml
    /// </summary>
    public partial class EditLoginConfig : Window
    {
        public EditLoginConfigVM ViewModel { get; private set; }

        public EditLoginConfig(EditLoginConfigVM vm)
        {
            InitializeComponent();
            DataContext = vm;
            ViewModel = vm;
        }
    }
}
