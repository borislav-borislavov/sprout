using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.SproutControlVMs;

namespace Sprout.Core.Factories
{
    public interface IDataServiceFactory
    {
        IDataService Create(IDataAdapter dataAdapter, VMRegistry vmRegistry);
    }
}