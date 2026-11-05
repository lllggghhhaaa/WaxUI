namespace WaxUI.Dropzone;

/// <summary>Where a <see cref="WaxDropOverlay"/> catches drops.</summary>
public enum WaxDropScope
{
    /// <summary>The whole window, shown as a fixed overlay above the page.</summary>
    Screen,

    /// <summary>Only the overlay's own wrapper, covering its ChildContent.</summary>
    Region,
}
