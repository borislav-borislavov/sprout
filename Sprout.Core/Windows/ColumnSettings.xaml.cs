using Sprout.Core.ViewModels;
using System.Windows;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for ColumnSettings.xaml
    /// </summary>
    public partial class ColumnSettings : Window
    {
        public ColumnSettingsVM ViewModel { get; private set; }

        public ColumnSettings(ColumnSettingsVM vm)
        {
            InitializeComponent();
            DataContext = vm;
            ViewModel = vm;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveCommand.Execute(null);
            DialogResult = ViewModel.IsSaved;
            Close();
        }
    }
}
