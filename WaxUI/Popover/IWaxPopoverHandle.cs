namespace WaxUI.Popover;

/// <summary>
/// A popover opened through <see cref="IWaxPopoverService"/>. Closing is idempotent;
/// <see cref="IsOpen"/> turns false as soon as the close is requested.
/// </summary>
public interface IWaxPopoverHandle
{
    Guid Id { get; }

    bool IsOpen { get; }

    Task CloseAsync();
}
