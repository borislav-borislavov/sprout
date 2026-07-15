using Sprout.Core.Features.ButtonActions;

namespace Sprout.Core.SproutControlVMs;

public interface IButtonActionHost
{
    Dictionary<string, IButtonAction> ButtonActions { get; }
}
