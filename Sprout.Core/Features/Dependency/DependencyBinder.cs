using Sprout.Core.Models;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Services
{
    public static class DependencyBinder
    {
        /// <summary>
        /// Creates bindings between each dependency from a dependent object and the corresponding
        /// property in the VM registry.
        /// </summary>
        /// <remarks>This method iterates through all dependencies defined in the dependent object and
        /// creates a binding for each one, linking it to the appropriate property in the VM registry. This
        /// enables automatic synchronization of VM with the underlying data model.</remarks>
        /// <param name="dependent">The dependent object containing the dependencies to bind. Must not be null.</param>
        /// <param name="vmRegistry">The VM registry that serves as the source for binding property values. Must not be null.</param>
        public static void BindDependencies(IDependent dependent, VMRegistry vmRegistry)
        {
            foreach (var dep in dependent.Dependencies)
            {
                BindingOperations.SetBinding(
                    target: dep,
                    DataProviderDependency.ValueProperty,
                    new Binding
                    {
                        Source = vmRegistry,
                        Path = new PropertyPath($"[{dep.ControlName}].{dep.PropertyPath}")
                    });
            }
        }
    }
}
