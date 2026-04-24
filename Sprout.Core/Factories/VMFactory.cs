using Microsoft.Extensions.DependencyInjection;
using Sprout.Core.Models.Configurations;
using Sprout.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
