using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public class LoginConfiguration : IDataAdapterControlConfig
    {
        public bool IsEnabled { get; set; } = true;
        public IDataAdapterConfig DataAdapter { get; set; }
    }
}
