using Sprout.Core.Models;
using Sprout.Core.Models.Configurations;
using Sprout.Core.UIStates;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sprout.Core.Views.Controls
{
    /// <summary>
    /// Interaction logic for SproutTextBox.xaml
    /// </summary>
    public partial class SproutTextBox : UserControl, ISproutControl<SproutTextBoxConfig>
    {
        public SproutTextBoxConfig Config { get; set; }
        public SproutControlType ControlType => SproutControlType.TextBox;
        public SproutTextBoxUIState UIState { get; internal set; }

        public SproutTextBox()
        {
            InitializeComponent();
            textBox.TextChanged += (s, e) => UpdatePlaceholderVisibility();
        }

        internal void SetPlaceholder(string placeholder)
        {
            lblPlaceholder.Text = placeholder;
            UpdatePlaceholderVisibility();
        }

        internal void EnableFileDrop()
        {
            textBox.AllowDrop = true;
            textBox.PreviewDragOver += OnTextBoxPreviewDragOver;
            textBox.PreviewDrop += OnTextBoxPreviewDrop;
        }

        private void OnTextBoxPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnTextBoxPreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                e.Data.GetData(DataFormats.FileDrop) is string[] files &&
                files.Length > 0)
            {
                textBox.Text = files[0];
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                e.Handled = true;
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            lblPlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
