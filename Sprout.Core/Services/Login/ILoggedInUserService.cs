

using Sprout.Core.Models.DataAdapters;

namespace Sprout.Core.Services.Login
{
    public interface ILoggedInUserService
    {
        IDataAdapter? UserDataAdapter { get; }

        void ResetUserData();
        void SetUserData(IDataAdapter loggedInUserDataAdapter);
    }
}