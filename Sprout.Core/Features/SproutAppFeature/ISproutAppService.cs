namespace Sprout.Core.Features.SproutAppFeature;

public interface ISproutAppService
{
    void CloseApp(bool force);
    void StartApp();
}