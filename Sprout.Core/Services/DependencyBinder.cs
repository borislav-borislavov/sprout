using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.UIStates;
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
        /// Creates bindings between each dependency from a data provider and the corresponding
        /// property in the UI state registry.
        /// </summary>
        /// <remarks>This method iterates through all dependencies defined in the data provider and
        /// creates a binding for each one, linking it to the appropriate property in the UI state registry. This
        /// enables automatic synchronization of UI state with the underlying data model.</remarks>
        /// <param name="dataProvider">The data provider containing the dependencies to bind. Must not be null.</param>
        /// <param name="uiStateRegistry">The UI state registry that serves as the source for binding property values. Must not be null.</param>
        public static void BindDependencies(IDataProvider dataProvider, UiStateRegistry uiStateRegistry)
        {
            foreach (var dep in dataProvider.Dependencies)
            {
                BindingOperations.SetBinding(
                    target: dep,
                    DataProviderDependency.ValueProperty,
                    new Binding
                    {
                        Source = uiStateRegistry,
                        Path = new PropertyPath($"[{dep.ControlName}].{dep.PropertyPath}")
                    });
            }
        }
    }
}
