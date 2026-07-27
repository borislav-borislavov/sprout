using System.Windows;

namespace Sprout.Core.Windows
{
    public partial class RenameTabWindow : Window
    {
        public string NewName => tbNewName.Text;

        public RenameTabWindow(string currentName)
        {
            InitializeComponent();
            tbNewName.Text = currentName;
            tbNewName.SelectAll();
            tbNewName.Focus();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
