namespace WaxUI.Popover;

/// <summary>
/// Options for a service-driven popover. Positioning mirrors <see cref="WaxPopover"/>:
/// the panel flips to the opposite side when the viewport is tight and stays within it.
/// </summary>
public sealed record WaxPopoverOptions
{
    /// <summary>Preferred side/alignment of the panel; flips when the viewport is tight.</summary>
    public WaxPopoverPlacement Placement { get; init; } = WaxPopoverPlacement.BottomStart;

    /// <summary>Gap between anchor and panel, in px.</summary>
    public int Offset { get; init; } = 8;

    /// <summary>Renders a small arrow on the panel edge facing the anchor.</summary>
    public bool ShowArrow { get; init; }

    /// <summary>Close when a pointer press lands outside the anchor and panel.</summary>
    public bool CloseOnOutsideClick { get; init; } = true;

    /// <summary>Close when Escape is pressed.</summary>
    public bool CloseOnEscape { get; init; } = true;

    /// <summary>Extra classes merged onto the panel (e.g. sizing and padding).</summary>
    public string Class { get; init; } = string.Empty;
}
