using Sprout.Core.Common;

namespace Sprout.Core.SproutControlVMs;

public class LoginVM() : BaseSproutControlVM(Const.Login, Guid.NewGuid())
{
    public object User { get; set; }
}
