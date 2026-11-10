namespace WaxUI.Menu;

/// <summary>
/// Options for a service-driven context menu. Coordinates are viewport pixels, exactly
/// like the <see cref="WaxMenu"/> component's <c>X</c>/<c>Y</c> parameters.
/// </summary>
public sealed record WaxMenuOptions
{
    public IReadOnlyList<WaxMenuItem> Items { get; init; } = [];

    /// <summary>Target display name for the header; omit (null) for a headerless menu.</summary>
    public string? HeaderLabel { get; init; }

    /// <summary>Menu category ("member", "channel"…) shown small and capitalized next to the header.</summary>
    public string HeaderKind { get; init; } = "member";

    /// <summary>Optional inline slider row; null hides it.</summary>
    public int? SliderValue { get; init; }

    public int SliderMin { get; init; }

    public int SliderMax { get; init; } = 200;

    public string SliderLabel { get; init; } = "Adjust";

    /// <summary>Fires while dragging (live application, no persistence).</summary>
    public Action<int>? SliderChanged { get; init; }

    /// <summary>Fires when the drag is released: the durable save moment.</summary>
    public Action<int>? SliderCommitted { get; init; }

    /// <summary>Extra classes merged onto the menu panel.</summary>
    public string Class { get; init; } = string.Empty;
}
