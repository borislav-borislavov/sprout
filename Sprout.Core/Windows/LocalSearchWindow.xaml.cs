using System.Windows;
using System.Windows.Input;

namespace Sprout.Core.Windows
{
    /// <summary>
    /// A lightweight Local Search prompt that asks the user for a value to search the
    /// grid's current data (all columns). Returns the entered text via <see cref="SearchText"/>.
    /// </summary>
    public partial class LocalSearchWindow : Window
    {
        /// <summary>
        /// The text the user wants to search for. An empty value clears the filter.
        /// </summary>
        public string SearchText { get; private set; } = string.Empty;

        public LocalSearchWindow(string initialText = "")
        {
            InitializeComponent();

            txtSearch.Text = initialText ?? string.Empty;
            Loaded += (_, _) =>
            {
                txtSearch.SelectAll();
                txtSearch.Focus();
            };
        }

        private void Confirm(string text)
        {
            SearchText = text ?? string.Empty;
            DialogResult = true;
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            Confirm(txtSearch.Text);
            e.Handled = true;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
            => Confirm(txtSearch.Text);

        private void btnClear_Click(object sender, RoutedEventArgs e)
            => Confirm(string.Empty);
    }
}
