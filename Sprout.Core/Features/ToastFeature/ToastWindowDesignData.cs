namespace Sprout.Core.Features.ToastFeature;

public sealed class ToastWindowDesignData
{
    public IReadOnlyList<ToastItemVM> Toasts { get; } =
    [
        new ToastItemVM(new ToastRequest
        {
            Title = "Sample notification",
            Message = "This is a sample toast notification.",
            Kind = ToastKind.Success
        }),
        new ToastItemVM(new ToastRequest
        {
            Message = "This is a sample toast notification.",
            Kind = ToastKind.Success
        }),
        new ToastItemVM(new ToastRequest
        {
            Title = "Sample notification",
            Message = "This is a sample toast warning.",
            Kind = ToastKind.Warning
        }),
        new ToastItemVM(new ToastRequest
        {
            Message = "This is a sample toast warning.",
            Kind = ToastKind.Warning
        }),
        new ToastItemVM(new ToastRequest
        {
            Title = "Sample notification",
            Message = "This is a sample toast error.",
            Kind = ToastKind.Error
        })
    ];
}
