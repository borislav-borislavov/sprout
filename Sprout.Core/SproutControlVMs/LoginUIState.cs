using Sprout.Core.Common;

namespace Sprout.Core.SproutControlVMs;

public class LoginUIState() : BaseSproutControlVM(Const.Login)
{
    public object User { get; set; }
}
