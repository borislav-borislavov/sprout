using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.SproutControlVMs;

namespace Sprout.Core.Models;

public interface IDependent
{
    IEnumerable<DataProviderDependency> Dependencies { get; }

    void DepenencyChanged(DataProviderDependency changedDependency, VMRegistry vmRegistry);
}
