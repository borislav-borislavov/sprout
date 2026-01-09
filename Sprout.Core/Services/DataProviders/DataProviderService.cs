using Sprout.Core.Models.Queries;
using Sprout.Core.Services.Queries;
using Sprout.Core.UIStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Services.DataProviders
{
    public class DataProviderService
    {
        public void ProvideData(IDataProvider dataProvider)
        {
            if (dataProvider is Query sqlServerDataProvider)
            {
                QueryService.ExecuteQuery(sqlServerDataProvider);
            }
            else
            {
                throw new NotImplementedException("DataProvider type not implemented");
            }
        }

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
