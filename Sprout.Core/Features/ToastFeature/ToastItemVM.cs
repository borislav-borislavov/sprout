using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Sprout.Core.Features.ToastFeature;

public partial class ToastItemVM : ObservableObject
{
    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private ToastKind _kind;

    [ObservableProperty]
    private Brush _borderBrush = Brushes.White;

    [ObservableProperty]
    private Brush _backgroundBrush = Brushes.WhiteSmoke;

    [ObservableProperty]
    private Brush _foregroundBrush = Brushes.Black;

    [ObservableProperty]
    private Visibility _titleVisibility = Visibility.Collapsed;

    public ToastItemVM(ToastRequest request)
    {
        Message = request.Message;
        Title = request.Title;
        Kind = request.Kind;
        ApplyStyle(request.Kind);
        TitleVisibility = string.IsNullOrWhiteSpace(request.Title) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ApplyStyle(ToastKind kind)
    {
        switch (kind)
        {
            case ToastKind.Success:
                BorderBrush = Brushes.SeaGreen;
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(245, 255, 247));
                break;
            case ToastKind.Warning:
                BorderBrush = Brushes.Goldenrod;
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 250, 235));
                break;
            case ToastKind.Error:
                BorderBrush = Brushes.IndianRed;
                BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 240, 240));
                break;
            default:
                BorderBrush = Brushes.White;
                BackgroundBrush = Brushes.WhiteSmoke;
                break;
        }
    }
}
