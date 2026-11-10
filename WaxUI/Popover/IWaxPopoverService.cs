using Microsoft.AspNetCore.Components;

namespace WaxUI.Popover;

/// <summary>
/// Opens popovers imperatively and anchors them to an element or CSS selector. A
/// <see cref="WaxPopoverOutlet"/> placed once in the interactive layout/page renders
/// whatever the service has open; without an outlet nothing is displayed.
/// </summary>
public interface IWaxPopoverService
{
    /// <summary>Raised (from any thread) whenever the open-popover list changes.</summary>
    event Action? Changed;

    /// <summary>Currently open popovers, oldest first.</summary>
    IReadOnlyList<WaxPopoverInstance> OpenPopovers { get; }

    /// <summary>Opens a popover anchored to an element reference.</summary>
    Task<IWaxPopoverHandle> ShowAsync(
        ElementReference anchor,
        RenderFragment content,
        WaxPopoverOptions? options = null);

    /// <summary>Opens a popover anchored to the first element matching a CSS selector.</summary>
    Task<IWaxPopoverHandle> ShowAsync(
        string anchorSelector,
        RenderFragment content,
        WaxPopoverOptions? options = null);

    /// <summary>Closes one popover; false when it was already closed.</summary>
    bool Close(IWaxPopoverHandle handle);

    /// <summary>Closes every open popover.</summary>
    void CloseAll();
}
