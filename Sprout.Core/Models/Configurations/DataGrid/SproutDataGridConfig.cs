using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations.DataGrid
{
    public class SproutDataGridConfig : SproutControlConfig
    {
        public string QueryName { get; set; }

        public bool AllowInsert { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }

        public bool ShowSave => AllowInsert || AllowUpdate || AllowDelete;

        public List<SproutDataGridColumnConfig> Columns { get; set; }
    }
}
