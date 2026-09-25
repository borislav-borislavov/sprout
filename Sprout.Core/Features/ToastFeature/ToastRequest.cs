namespace Sprout.Core.Features.ToastFeature;

public enum ToastKind
{
    Info,
    Success,
    Warning,
    Error
}

public sealed class ToastRequest
{
    public required string Message { get; init; }

    public string? Title { get; init; }

    public ToastKind Kind { get; init; } = ToastKind.Info;

    public TimeSpan? Duration { get; init; }
}
