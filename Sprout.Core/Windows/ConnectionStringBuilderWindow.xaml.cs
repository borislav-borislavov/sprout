using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Sprout.Core.Windows
{
    public partial class ConnectionStringBuilderWindow : Window
    {
        private bool _initialized;
        private bool _databasesLoaded;

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
                cmbDatabase.Text = builder.InitialCatalog;
                cmbAuth.SelectedIndex = builder.IntegratedSecurity ? 0 : 1;
                txtUser.Text = builder.UserID;
                txtPassword.Password = builder.Password;
                chkEncrypt.IsChecked = builder.Encrypt != SqlConnectionEncryptOption.Optional;
                chkTrustCert.IsChecked = builder.TrustServerCertificate;

                txtConnectTimeout.Text = builder.ConnectTimeout.ToString();
                txtCommandTimeout.Text = builder.CommandTimeout.ToString();
                txtAppName.Text = builder.ShouldSerialize("Application Name") ? builder.ApplicationName : string.Empty;
                cmbAppIntent.SelectedIndex = builder.ApplicationIntent == ApplicationIntent.ReadOnly ? 1 : 0;
                txtPacketSize.Text = builder.PacketSize.ToString();
                txtRetryCount.Text = builder.ConnectRetryCount.ToString();
                txtRetryInterval.Text = builder.ConnectRetryInterval.ToString();
                chkPooling.IsChecked = builder.Pooling;
                chkMars.IsChecked = builder.MultipleActiveResultSets;
                txtMinPoolSize.Text = builder.MinPoolSize.ToString();
                txtMaxPoolSize.Text = builder.MaxPoolSize.ToString();
            }
            catch
            {
                // Invalid connection string - start from scratch.
            }
        }

        private bool IsWindowsAuth => cmbAuth.SelectedIndex == 0;

        private static int ParseOr(string text, int fallback)
            => int.TryParse(text?.Trim(), out var value) && value >= 0 ? value : fallback;

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

            if (!string.IsNullOrWhiteSpace(cmbDatabase.Text))
                builder.InitialCatalog = cmbDatabase.Text.Trim();

            if (!IsWindowsAuth)
            {
                builder.UserID = txtUser.Text.Trim();
                builder.Password = txtPassword.Password;
            }

            ApplyAdvancedOptions(builder);

            return builder.ConnectionString;
        }

        private void ApplyAdvancedOptions(SqlConnectionStringBuilder builder)
        {
            // Only emit keywords whose values differ from the SqlClient defaults
            // so the resulting connection string stays clean.
            var connectTimeout = ParseOr(txtConnectTimeout.Text, 15);
            if (connectTimeout != 15)
                builder.ConnectTimeout = connectTimeout;

            var commandTimeout = ParseOr(txtCommandTimeout.Text, 30);
            if (commandTimeout != 30)
                builder.CommandTimeout = commandTimeout;

            if (!string.IsNullOrWhiteSpace(txtAppName.Text))
                builder.ApplicationName = txtAppName.Text.Trim();

            if (cmbAppIntent.SelectedIndex == 1)
                builder.ApplicationIntent = ApplicationIntent.ReadOnly;

            var packetSize = ParseOr(txtPacketSize.Text, 8000);
            if (packetSize != 8000)
                builder.PacketSize = packetSize;

            var retryCount = ParseOr(txtRetryCount.Text, 1);
            if (retryCount != 1)
                builder.ConnectRetryCount = retryCount;

            var retryInterval = ParseOr(txtRetryInterval.Text, 10);
            if (retryInterval != 10)
                builder.ConnectRetryInterval = retryInterval;

            if (chkPooling.IsChecked != true)
                builder.Pooling = false;

            if (chkMars.IsChecked == true)
                builder.MultipleActiveResultSets = true;

            var minPoolSize = ParseOr(txtMinPoolSize.Text, 0);
            if (minPoolSize != 0)
                builder.MinPoolSize = minPoolSize;

            var maxPoolSize = ParseOr(txtMaxPoolSize.Text, 100);
            if (maxPoolSize != 100)
                builder.MaxPoolSize = maxPoolSize;
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

        private void Input_Changed(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, txtServer) || ReferenceEquals(sender, txtUser) || ReferenceEquals(sender, txtPassword) || ReferenceEquals(sender, cmbAuth))
                _databasesLoaded = false;

            UpdatePreview();
        }

        private void Auth_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
                return;

            _databasesLoaded = false;
            UpdateAuthVisibility();
            UpdatePreview();
        }

        private async void Database_DropDownOpened(object sender, EventArgs e)
        {
            if (_databasesLoaded || string.IsNullOrWhiteSpace(txtServer.Text))
                return;

            try
            {
                var builder = new SqlConnectionStringBuilder(BuildConnectionString())
                {
                    InitialCatalog = "master",
                    ConnectTimeout = 10
                };

                var current = cmbDatabase.Text;
                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand("SELECT name FROM sys.databases ORDER BY name", connection);
                var databases = new List<string>();
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    databases.Add(reader.GetString(0));

                cmbDatabase.ItemsSource = databases;
                cmbDatabase.Text = current;
                _databasesLoaded = true;
            }
            catch
            {
                // Could not load databases - user can still type the name manually.
            }
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
