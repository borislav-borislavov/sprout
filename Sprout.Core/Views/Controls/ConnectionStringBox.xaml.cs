using System.Windows;
using System.Windows.Controls;
using Sprout.Core.Windows;

namespace Sprout.Core.Views.Controls
{
    public partial class ConnectionStringBox : UserControl
    {
        public static readonly DependencyProperty ConnectionStringProperty =
            DependencyProperty.Register(
                nameof(ConnectionString),
                typeof(string),
                typeof(ConnectionStringBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(ConnectionStringBox),
                new PropertyMetadata("SQL Server Connection String", OnLabelChanged));

        public string ConnectionString
        {
            get => (string)GetValue(ConnectionStringProperty);
            set => SetValue(ConnectionStringProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public ConnectionStringBox()
        {
            InitializeComponent();
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ConnectionStringBox)d).lblTitle.Text = e.NewValue as string ?? string.Empty;
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            var window = new ConnectionStringBuilderWindow(ConnectionString)
            {
                Owner = Window.GetWindow(this)
            };

            if (window.ShowDialog() == true)
            {
                ConnectionString = window.ConnectionString;
            }
        }
    }
}
