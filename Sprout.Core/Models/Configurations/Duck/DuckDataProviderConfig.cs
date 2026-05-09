using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.Duck
{
    public class DuckDataProviderConfig : IDataProviderConfig
    {
        public string Text { get; set; }

        public ObservableCollection<FilterConfig> FilterConfigs { get; set; } = [];
    }
}