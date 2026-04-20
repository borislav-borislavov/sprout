using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Xaml.Behaviors.Core;
using Sprout.Core.Messages;
using Sprout.Core.Services.Configurations;
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
    public partial class LoginVM : ObservableObject
    {
        private readonly ILoginService _loginService;
        private readonly ILoggedInUserService _loggedInUserService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IConfigurationService _configurationService;

        public LoginVM(
            ILoginService loginService,
            ILoggedInUserService loggedInUserService,
            IDialogService dialogService,
            INavigationService navigationService,
            IConfigurationService configurationService)
        {
            _loginService = loginService;
            _loggedInUserService = loggedInUserService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _configurationService = configurationService;

            var config = _configurationService.Load();
            UserName = config.Settings.LastUsername;
        }

        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        [ObservableProperty]
        private string _userName;

        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _showPassword;

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

                    var config = _configurationService.Load();
                    config.Settings.LastUsername = UserName;
                    _configurationService.Save(config);

                    _navigationService.ShowMainDashboard();
                    WeakReferenceMessenger.Default.Send(new CloseWindowMessage());

                    //throw new Exception("TODO:
                    //make a property for enabling login,
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
