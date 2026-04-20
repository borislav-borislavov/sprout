using CommunityToolkit.Mvvm.Messaging;
using Sprout.Core.Messages;
using Sprout.Core.ViewModels;
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
using System.Windows.Shapes;

namespace Sprout.Core.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginVM vm)
        {
            InitializeComponent();
            DataContext = vm;

            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (r, m) =>
            {
                OnCloseWindow();
            });

            Loaded += (s, e) =>
            {
                if (!string.IsNullOrEmpty(vm.UserName))
                    pwdBox.Focus();
            };
        }

        private void OnCloseWindow()
        {
            Close();
        }

        private void pwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is not LoginVM loginVM)
                return;

            loginVM.Password = pwdBox.Password;
        }

        private void pwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (DataContext is not LoginVM loginVM)
                return;

            if (loginVM.LoginCommand.CanExecute(null))
                loginVM.LoginCommand.Execute(null);
        }
    }
}
