using Sprout.Core.Models.Configurations.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SproutPageConfiguration
    {
        public string Title { get; set; }

        public SproutControlConfig Root { get; set; }

        public List<IDataProviderConfig> GetDataProviders()
        {
            var dataProviders = new List<IDataProviderConfig>();
            if (Root is null || Root is not GridConfig rootGridConfig)
            {
                return dataProviders;
            }

            GetDataProvidersRecursive(Root, dataProviders);
            return dataProviders;
        }

        private void GetDataProvidersRecursive(
            SproutControlConfig control,
            List<IDataProviderConfig> dataProviders)
        {
            if (control is GridConfig grid)
            {
                foreach (var child in grid.Children)
                {
                    GetDataProvidersRecursive(child, dataProviders);
                }
            }
            else if (control is IDataRetreiver dataRetreiverControl)
            {
                if (dataRetreiverControl.DataProviderConfig is not null)
                {
                    dataProviders.Add(dataRetreiverControl.DataProviderConfig);
                }
            }
        }
    }
}
