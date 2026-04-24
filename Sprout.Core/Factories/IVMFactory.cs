using Sprout.Core.Models.Configurations;
using Sprout.Core.ViewModels;

namespace Sprout.Core.Factories
{
    public interface IVMFactory
    {
        T Create<T>(params object[] parameters) where T : class;
    }
}