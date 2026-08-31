using System.Windows;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// A minimal OK/Cancel dialog asking the user for a single text value.
    /// </summary>
    public partial class TextInputWindow : Window
    {
        public string Value => tbValue.Text;

        public TextInputWindow(string title, string prompt, string initialValue = "")
        {
            InitializeComponent();
            Title = title;
            tbPrompt.Text = prompt;
            tbValue.Text = initialValue;
            tbValue.SelectAll();
            tbValue.Focus();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
