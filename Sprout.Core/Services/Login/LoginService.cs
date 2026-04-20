using Sprout.Core.Factories;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.Configurations;
using Sprout.Core.Services.Dialog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDataAdapter = Sprout.Core.Models.DataAdapters.IDataAdapter;

namespace Sprout.Core.Services.Login
{
    public class LoginService : ILoginService
    {
        private readonly IDataAdapter _loginDataAdapter;
        private readonly IDataServiceFactory _dataServiceFactory;

        public LoginService(IConfigurationService configurationService,
            IDataAdapterFactory dataAdapterFactory,
            IDataServiceFactory dataServiceFactory,
            IDialogService dialogService)
        {
            try
            {
                var sproutConfig = configurationService.Load();

                if (sproutConfig.Login is not LoginConfiguration loginConfig)
                {
                    throw new Exception($"Failed to load {nameof(LoginConfiguration)}");
                }

                _loginDataAdapter = dataAdapterFactory.Create(loginConfig.DataAdapter);
                _dataServiceFactory = dataServiceFactory;
            }
            catch (Exception ex)
            {
                dialogService.ShowError($"{ex}");
            }
        }

        public async Task<LoginResult> Login(LoginCommand login)
        {
            var loginResult = new LoginResult();

            using (var loginDataService = _dataServiceFactory.Create(_loginDataAdapter, new UIStates.UiStateRegistry()))
            {
                var parameter = GetLoginParameter(login);

                var changeResult = await loginDataService.Update(parameter);

                if (changeResult.Result == null)
                {
                    loginResult.Result = false;
                    loginResult.ErrorMessage = "Make sure to 'SELECT 1 AS Result' for the status of the operation";
                    return loginResult;
                }

                if (changeResult.Result == true)
                {
                    if (changeResult.ExtraData == null || changeResult.ExtraData.Rows.Count != 1)
                    {
                        loginResult.Result = false;
                        loginResult.ErrorMessage = "Make sure to select the needed UserData";
                        return loginResult;
                    }

                    loginResult.Result = true;
                    _loginDataAdapter.DataProvider.Data = changeResult.ExtraData;
                    loginResult.LoginDataAdapter = _loginDataAdapter;
                    return loginResult;
                }
                else 
                {
                    loginResult.Result = false;

                    if (changeResult.Messages.Any())
                    {
                        loginResult.ErrorMessage = string.Join(Environment.NewLine, changeResult.Messages.Select(m => m.Message));
                    }
                    else
                    {
                        loginResult.ErrorMessage = "Result was set to false but no message was provided";
                    }

                    return loginResult;
                }
            }
        }

        private static DataRow GetLoginParameter(LoginCommand login)
        {
            var dt = new DataTable();
            dt.Columns.Add(nameof(login.Username), typeof(string));
            dt.Columns.Add(nameof(login.Password), typeof(string));

            return dt.Rows.Add([login.Username, login.Password]);
        }
    }
}
