using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.UIStates;

namespace Sprout.Core.Factories
{
    public interface IDataServiceFactory
    {
        IDataService Create(IDataAdapter dataAdapter, UiStateRegistry uiStateRegistry);
    }
}