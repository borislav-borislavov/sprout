using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SqlServerDataProviderConfig : IDataProviderConfig
    {
        public string Text { get; set; }

        public ObservableCollection<SqlServerFilterConfig> FilterConfigs { get; set; } = [];
    }
}
