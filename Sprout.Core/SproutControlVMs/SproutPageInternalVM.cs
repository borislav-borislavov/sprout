using Sprout.Core.Common;

namespace Sprout.Core.SproutControlVMs;

public class SproutPageInternalVM(Guid pageId) : BaseSproutControlVM(Const.Page, pageId)
{
    public object Data { get; set; }
}
