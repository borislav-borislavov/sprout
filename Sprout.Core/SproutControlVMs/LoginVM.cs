using Sprout.Core.Common;

namespace Sprout.Core.SproutControlVMs;

public class LoginVM() : BaseSproutControlVM(Const.Login)
{
    public object User { get; set; }
}
