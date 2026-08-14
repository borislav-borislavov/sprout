using System.Threading.Tasks;

namespace Sprout.Core.Services.Updates
{
    public interface IUpdateService
    {
        Task CheckForUpdatesAsync();
    }
}
