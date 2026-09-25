namespace Sprout.Core.Features.ToastFeature;

public interface IToastService
{
    void Show(ToastRequest request);

    void ShowInfo(string message, string? title = null, TimeSpan? duration = null);

    void ShowSuccess(string message, string? title = null, TimeSpan? duration = null);

    void ShowWarning(string message, string? title = null, TimeSpan? duration = null);

    void ShowError(string message, string? title = null, TimeSpan? duration = null);
}
