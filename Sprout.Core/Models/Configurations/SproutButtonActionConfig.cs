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

    public class ExecuteSelectActionConfig : SproutButtonActionConfig
    {
    }

    public class CopyToClipboardActionConfig : SproutButtonActionConfig
    {
        public string ClipboardText { get; set; }
    }

    public class OpenPageActionConfig : SproutButtonActionConfig
    {
        public Guid PageID { get; set; }
        public bool OpenAsDialog { get; set; }
    }

    public class ClosePageActionConfig : SproutButtonActionConfig
    {
        public Guid PageID { get; set; }
    }
}
