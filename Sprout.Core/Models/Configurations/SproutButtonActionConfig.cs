using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
    public abstract class SproutButtonActionConfig
    {
    }

    public class ExecuteUpdateActionConfig : SproutButtonActionConfig
    {
    }

    public class RefreshDataGridActionConfig : SproutButtonActionConfig
    {
        public string TargetDataGridName { get; set; }
    }
}
