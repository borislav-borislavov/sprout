using Sprout.Core.Models.DataAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Services.Login
{
    public class LoggedInUserService : ILoggedInUserService
    {
        public Models.DataAdapters.IDataAdapter? UserDataAdapter { get; private set; }

        public void SetUserData(IDataAdapter adapter)
        {
            UserDataAdapter = adapter;
        }

        public void ResetUserData()
        {
            UserDataAdapter = null;
        }
    }
}
