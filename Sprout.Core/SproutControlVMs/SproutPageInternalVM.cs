using Sprout.Core.Common;

namespace Sprout.Core.SproutControlVMs;

public class SproutPageInternalVM() : BaseSproutControlVM(Const.Page)
{
    public object Data { get; set; }
}
