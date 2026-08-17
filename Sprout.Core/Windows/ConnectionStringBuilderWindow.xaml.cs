using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Sprout.Core.Windows
{
    public partial class ConnectionStringBuilderWindow : Window
    {
        private bool _initialized;

        public string ConnectionString { get; private set; } = string.Empty;

        public ConnectionStringBuilderWindow(string connectionString)
        {
            InitializeComponent();

            LoadFrom(connectionString);
            _initialized = true;

            UpdateAuthVisibility();
            UpdatePreview();
        }

        private void LoadFrom(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                txtServer.Text = builder.DataSource;
                txtDatabase.Text = builder.InitialCatalog;
                cmbAuth.SelectedIndex = builder.IntegratedSecurity ? 0 : 1;
                txtUser.Text = builder.UserID;
                txtPassword.Password = builder.Password;
                chkEncrypt.IsChecked = builder.Encrypt != SqlConnectionEncryptOption.Optional;
                chkTrustCert.IsChecked = builder.TrustServerCertificate;
            }
            catch
            {
                // Invalid connection string - start from scratch.
            }
        }

        private bool IsWindowsAuth => cmbAuth.SelectedIndex == 0;

        private string BuildConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = txtServer.Text.Trim(),
                IntegratedSecurity = IsWindowsAuth,
                Encrypt = chkEncrypt.IsChecked == true
                    ? SqlConnectionEncryptOption.Mandatory
                    : SqlConnectionEncryptOption.Optional,
                TrustServerCertificate = chkTrustCert.IsChecked == true
            };

            if (!string.IsNullOrWhiteSpace(txtDatabase.Text))
                builder.InitialCatalog = txtDatabase.Text.Trim();

            if (!IsWindowsAuth)
            {
                builder.UserID = txtUser.Text.Trim();
                builder.Password = txtPassword.Password;
            }

            return builder.ConnectionString;
        }

        private void UpdateAuthVisibility()
        {
            var visibility = IsWindowsAuth ? Visibility.Collapsed : Visibility.Visible;
            lblUser.Visibility = visibility;
            txtUser.Visibility = visibility;
            lblPassword.Visibility = visibility;
            txtPassword.Visibility = visibility;
        }

        private void UpdatePreview()
        {
            if (!_initialized)
                return;

            try
            {
                txtPreview.Text = BuildConnectionString();
            }
            catch
            {
                txtPreview.Text = string.Empty;
            }
        }

        private void Input_Changed(object sender, RoutedEventArgs e) => UpdatePreview();

        private void Auth_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
                return;

            UpdateAuthVisibility();
            UpdatePreview();
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show(this, "Please enter a server name.", "Test Connection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnTest.IsEnabled = false;
            try
            {
                var builder = new SqlConnectionStringBuilder(BuildConnectionString())
                {
                    ConnectTimeout = 10
                };

                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                MessageBox.Show(this, "Connection succeeded.", "Test Connection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Connection failed:\n\n{ex.Message}", "Test Connection",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnTest.IsEnabled = true;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show(this, "Please enter a server name.", "Connection String",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ConnectionString = BuildConnectionString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Connection String",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }
    }
}
