using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sprout.Core.Features.ToastFeature;

public partial class ToastWindow : Window
{
    private readonly ToastService _toastService;

    public ToastWindow(ToastService toastService)
    {
        InitializeComponent();
        _toastService = toastService;
        DataContext = toastService;

        ((INotifyCollectionChanged)toastService.Toasts).CollectionChanged += (_, _) =>
        {
            if (toastService.Toasts.Count == 0)
            {
                Hide();
            }
        };
    }

    public void ShowCentered()
    {
        if (!_toastService.Toasts.Any())
        {
            return;
        }

        UpdateLayout();
        CenterToScreen();

        if (!IsVisible)
        {
            Show();
        }
    }

    private void CloseToast_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ToastItemVM toast)
        {
            return;
        }

        _toastService.Remove(toast);
    }

    private void MessageText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2)
        {
            return;
        }

        if (sender is not FrameworkElement element || element.DataContext is not ToastItemVM toast)
        {
            return;
        }

        Clipboard.SetText(toast.Message);
        e.Handled = true;
    }

    private void CenterToScreen()
    {
        var workArea = SystemParameters.WorkArea;
        var width = ActualWidth > 0 ? ActualWidth : (double.IsNaN(Width) || Width <= 0 ? 360 : Width);
        var height = ActualHeight > 0 ? ActualHeight : (double.IsNaN(Height) || Height <= 0 ? 120 : Height);

        Left = workArea.Left + Math.Max(0, (workArea.Width - width) / 2);
        Top = workArea.Top + Math.Max(0, (workArea.Height - height) / 2);
    }
}
