using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.SqlServer;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Services.DataProviders
{
    public interface IDataService : IDisposable
    {
        Task ProvideData();

        Task Insert(DataRow dataRow);
        Task Update(DataRow dataRow);
        Task Delete(DataRow dataRow);

        public void BindDependencies(IDataProvider dataProvider, UiStateRegistry uiStateRegistry)
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
