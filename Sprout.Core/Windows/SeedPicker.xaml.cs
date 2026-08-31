using Sprout.Core.Models.Configurations;
using Sprout.Core.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// Interaction logic for SeedPicker.xaml
    /// </summary>
    public partial class SeedPicker : Window
    {
        private readonly SeedPickerVM _vm;
        public SeedFile SelectedSeed => _vm?.SelectedSeed;

        public SeedPicker(IEnumerable<SeedFile> seedFiles)
        {
            InitializeComponent();
            _vm = new SeedPickerVM();
            _vm.LoadSeeds(seedFiles);
            DataContext = _vm;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedSeed == null)
                return;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.SelectedSeed = null;
            DialogResult = false;
            Close();
        }

        private void LbSeeds_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_vm.SelectedSeed == null)
                return;

            DialogResult = true;
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (DialogResult != true)
            {
                _vm.SelectedSeed = null;
            }
        }
    }
}
