using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace Sprout.Core.Features.ToastFeature;

public sealed class ToastService : IToastService
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(3);
    private readonly IServiceProvider _serviceProvider;
    private readonly ObservableCollection<ToastItemVM> _toasts = [];

    public ReadOnlyObservableCollection<ToastItemVM> Toasts { get; }

    public ToastService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Toasts = new ReadOnlyObservableCollection<ToastItemVM>(_toasts);

        ((INotifyCollectionChanged)_toasts).CollectionChanged += (_, _) =>
        {
            ExecuteOnUiThread(() =>
            {
                if (_toasts.Count == 0)
                {
                    TryGetWindow()?.Hide();
                }
            });
        };
    }

    public void Show(ToastRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return;
        }

        var toast = new ToastItemVM(request);
        ExecuteOnUiThread(() => _toasts.Insert(0, toast));

        var window = TryGetWindow();
        window?.ShowCentered();

        var duration = request.Duration ?? DefaultDuration;

        _ = DismissLaterAsync(toast, duration);
    }

    public void ShowInfo(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastRequest { Message = message, Title = title, Kind = ToastKind.Info, Duration = duration });

    public void ShowSuccess(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastRequest { Message = message, Title = title, Kind = ToastKind.Success, Duration = duration });

    public void ShowWarning(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastRequest { Message = message, Title = title, Kind = ToastKind.Warning, Duration = duration });

    public void ShowError(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastRequest { Message = message, Title = title, Kind = ToastKind.Error, Duration = duration });

    public void Remove(ToastItemVM toast)
    {
        ExecuteOnUiThread(() => _toasts.Remove(toast));
    }

    private async Task DismissLaterAsync(ToastItemVM toast, TimeSpan duration)
    {
        try
        {
            await Task.Delay(duration).ConfigureAwait(false);
            Remove(toast);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private ToastWindow? TryGetWindow()
    {
        try
        {
            return _serviceProvider.GetService(typeof(ToastWindow)) as ToastWindow;
        }
        catch
        {
            return null;
        }
    }

    private static void ExecuteOnUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }
}
