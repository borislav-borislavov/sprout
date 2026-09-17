using Microsoft.Extensions.DependencyInjection;

namespace Sprout.Core.Factories
{
    public class VMFactory(IServiceProvider _serviceProvider) : IVMFactory
    {
        public T Create<T>(params object[] parameters) where T : class
        {
            return ActivatorUtilities.CreateInstance<T>(_serviceProvider, parameters);
        }
    }
}
