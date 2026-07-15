using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.UIStates;

namespace Sprout.Core.Models;

public interface IDependent
{
    IEnumerable<DataProviderDependency> Dependencies { get; }

    void DepenencyChanged(DataProviderDependency changedDependency, VMRegistry vmRegistry);
}
