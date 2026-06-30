using Sprout.Core.Services.CPL;
using Sprout.Core.ViewModels;
using System.Windows;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for ManageUsingsWindow.xaml
    /// </summary>
    public partial class ManageUsingsWindow : Window
    {
        public ManageUsingsVM ViewModel { get; private set; }

        public ManageUsingsWindow(ManageUsingsVM vm)
        {
            InitializeComponent();
            DataContext = vm;
            ViewModel = vm;
        }

        public void Initialize(BaseCompiler compiler)
        {
            ViewModel.Initialize(compiler);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyCommand.Execute(null);
            DialogResult = true;
            Close();
        }
    }
}
