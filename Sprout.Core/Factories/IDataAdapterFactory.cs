using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.DataAdapters;

namespace Sprout.Core.Factories
{
    public interface IDataAdapterFactory
    {
        IDataAdapter Create(IDataAdapterConfig dataAdapterConfig);
    }
}
