using Sprout.Core.Features.ButtonActions;

namespace Sprout.Core.UIStates;

public interface IButtonActionHost
{
    Dictionary<string, IButtonAction> ButtonActions { get; }
}
