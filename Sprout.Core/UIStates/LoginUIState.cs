using Sprout.Core.Common;

namespace Sprout.Core.UIStates;

public class LoginUIState() : BaseSproutControlVM(Const.Login)
{
    public object User { get; set; }
}
