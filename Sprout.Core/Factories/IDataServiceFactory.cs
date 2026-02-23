using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.DataProviders;

namespace Sprout.Core.Factories
{
    public interface IDataServiceFactory
    {
        IDataService Create(IDataAdapter dataAdapter);
    }
}