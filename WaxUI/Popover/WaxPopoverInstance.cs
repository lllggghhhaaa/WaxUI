using Microsoft.AspNetCore.Components;

namespace WaxUI.Popover;

/// <summary>One popover shown by <see cref="WaxPopoverOutlet"/>.</summary>
public sealed class WaxPopoverInstance : IWaxPopoverHandle
{
    private readonly TaskCompletionSource _closed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal WaxPopoverInstance(object anchor, RenderFragment body, WaxPopoverOptions options)
    {
        Anchor = anchor;
        Body = body;
        Options = options;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public bool IsOpen => !_closed.Task.IsCompleted;

    /// <summary>The anchor passed by the caller: an <see cref="ElementReference"/> or a CSS selector.</summary>
    internal object Anchor { get; }

    /// <summary>The popover content, rendered by the outlet.</summary>
    internal RenderFragment Body { get; }

    public WaxPopoverOptions Options { get; }

    /// <summary>Completes when the popover is closed.</summary>
    internal Task Closed => _closed.Task;

    public Task CloseAsync()
    {
        _closed.TrySetResult();
        return Task.CompletedTask;
    }
}
