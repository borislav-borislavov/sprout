using Sprout.Core.Models.DataAdapters;
using Sprout.Core.SproutControlVMs;

namespace Sprout.Core.Common
{
    public static class VMRegistryExtensions
    {
        public static IDataAdapter GetAdapterOrThrow(this VMRegistry vmRegistry, string controlName)
        {
            var vm = vmRegistry[controlName];

            if (vm is not IDataAdapterHost dataAdapterHost)
                throw new Exception($"{controlName} is not a DataAdapterHost");

            return dataAdapterHost.DataAdapter;
        }
    }
}
