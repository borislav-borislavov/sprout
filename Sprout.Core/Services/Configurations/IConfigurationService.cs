using Sprout.Core.Models.Configurations;

namespace Sprout.Core.Services.Configurations
{
    public interface IConfigurationService
    {
        SproutConfiguration Load();
        SproutConfiguration LoadSpecific(string identifier);
        bool Save(SproutConfiguration sproutConfiguration);
        bool CreateNew(string identifier);
        string GetIdentifier();
    }
}