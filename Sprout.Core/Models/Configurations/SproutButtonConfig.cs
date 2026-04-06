using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class SproutButtonConfig : SproutControlConfig, IDataAdapterControlConfig
    {
        public string Content { get; set; }

        public IDataAdapterConfig DataAdapter { get; set; }

        public ObservableCollection<SproutButtonActionConfig> Actions { get; set; } = [];
    }
}
