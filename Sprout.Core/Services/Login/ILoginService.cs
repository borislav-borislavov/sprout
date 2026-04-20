namespace Sprout.Core.Services.Login
{
    public interface ILoginService
    {
        Task<LoginResult> Login(LoginCommand login);
    }
}