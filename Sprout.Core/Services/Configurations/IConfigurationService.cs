using Sprout.Core.Models.Configurations;

namespace Sprout.Core.Services.Configurations
{
    public interface IConfigurationService
    {
        SproutConfiguration Load();
        bool Save(SproutConfiguration sproutConfiguration);
    }
}