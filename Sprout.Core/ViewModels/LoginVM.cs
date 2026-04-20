using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Xaml.Behaviors.Core;
using Sprout.Core.Messages;
using Sprout.Core.Services.Dialog;
using Sprout.Core.Services.Login;
using Sprout.Core.Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.ViewModels
{
    public partial class LoginVM(
        ILoginService _loginService,
        ILoggedInUserService _loggedInUserService,
        IDialogService _dialogService,
        INavigationService _navigationService
        ) : ObservableObject
    {
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        [ObservableProperty]
        private string _userName;

        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isBusy;

        [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(CanExecuteLogin))]
        private async Task Login()
        {
            IsBusy = true;

            try
            {
                var loginResult = await _loginService.Login(new()
                {
                    Username = UserName,
                    Password = Password
                });

                if (loginResult.Result)
                {
                    _loggedInUserService.SetUserData(loginResult.LoginDataAdapter);
                    _navigationService.ShowMainDashboard();
                    WeakReferenceMessenger.Default.Send(new CloseWindowMessage());

                    //throw new Exception("TODO:
                    //save last successful login username,
                    //make a property for enabling login,
                    //make a preview password button,
                    //use the IsBusy to disable the page for the duration of the login");
                }
                else
                {
                    _loggedInUserService.ResetUserData();
                    _dialogService.ShowError(loginResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Exception: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanExecuteLogin()
        {
            if (IsBusy)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(UserName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                return false;
            }

            return true;
        }
    }
}
