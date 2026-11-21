using Khamsin;
using WaxUI.Utils;

namespace WaxUI.ColorPicker;

/// <summary>
/// Utility classes for WaxColorPicker and WaxGradientColorPicker, wrapped in Cn.Of so the build extracts them. Colors come
/// only from Theme/tokens.css; the alpha checkerboard uses --color-border so it stays visible over
/// card, popover and muted surfaces in both themes.
/// </summary>
internal static class WaxColorPickerStyles
{
    /// <summary>Transparent/colored square pattern shown under translucent swatches and the alpha track.</summary>
    public static readonly KhamsinClass AlphaChecker =
        Cn.Of("bg-[repeating-conic-gradient(var(--color-border)_0%_25%,transparent_0%_50%)] bg-[length:10px_10px]").Value;

    /// <summary>Picker shell (a fieldset); position and width are appended per variant.</summary>
    public static readonly KhamsinClass Panel =
        Cn.Of("m-0 min-w-0 rounded-2xl border border-border bg-card p-3.5 shadow-xl shadow-black/5").Value;

    public static readonly KhamsinClass SvField =
        Cn.Of("relative h-40 w-full cursor-crosshair touch-none select-none overflow-hidden rounded-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring").Value;

    public static readonly KhamsinClass SvThumb =
        Cn.Of("pointer-events-none absolute h-3.5 w-3.5 -translate-x-1/2 -translate-y-1/2 rounded-full border-2 border-white shadow-sm").Value;

    public static readonly KhamsinClass Swatch =
        Cn.Of("inline-block shrink-0 border border-black/10 dark:border-white/10").Value;

    public static readonly KhamsinClass PresetButton =
        Cn.Of("block cursor-pointer transition-transform hover:scale-105 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring").Value;

    public static readonly KhamsinClass PopoverTrigger =
        Cn.Of("flex h-10 w-full items-center gap-2.5 rounded-xl border border-border bg-card px-3 text-left text-sm text-foreground transition-colors hover:border-foreground/30 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60").Value;

    public static readonly KhamsinClass SwatchTrigger =
        Cn.Of("inline-flex shrink-0 cursor-pointer transition-transform hover:scale-105 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-60").Value;

    public static readonly KhamsinClass FormatTrigger =
        Cn.Of("flex cursor-pointer items-center gap-1 rounded-md px-1 py-0.5 text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed").Value;

    public static readonly KhamsinClass FormatMenu =
        Cn.Of("absolute left-0 top-full z-[95] mt-1.5 w-28 rounded-xl border border-border bg-popover p-1 text-popover-foreground shadow-2xl shadow-black/15",
            WaxMotion.DropdownEnter).Value;

    public static readonly KhamsinClass FormatOption =
        Cn.Of("block w-full cursor-pointer rounded-lg px-3 py-1.5 text-left text-sm transition-colors hover:bg-muted").Value;

    public static readonly KhamsinClass HexInput =
        Cn.Of("w-20 min-w-0 bg-transparent text-center font-light text-base tracking-wide text-foreground outline-none disabled:cursor-not-allowed").Value;

    /// <summary>Hex readout column; wider than the numeric channel columns beside it (position, alpha).</summary>
    public static readonly KhamsinClass HexField =
        Cn.Of("flex-1 min-w-32").Value;

    public static readonly KhamsinClass ChannelLabel =
        Cn.Of("text-[10px] uppercase tracking-wide text-muted-foreground").Value;

    /// <summary>Invisible full-screen backdrop used to close the floating panel and the format menu.</summary>
    public static readonly KhamsinClass ClickAway =
        Cn.Of("fixed inset-0 z-[94]").Value;

    /// <summary>Discreet segmented control container (solid/gradient mode toggle, gradient type selector).</summary>
    public static readonly KhamsinClass SegmentGroup =
        Cn.Of("inline-flex shrink-0 items-center gap-0.5 rounded-lg bg-muted/60 p-0.5").Value;

    public static readonly KhamsinClass SegmentButton =
        Cn.Of("flex cursor-pointer items-center justify-center rounded-md px-2 py-1 text-xs font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60").Value;

    /// <summary>Selected option inside a segmented control.</summary>
    public static readonly KhamsinClass SegmentButtonActive =
        Cn.Of("bg-card text-foreground shadow-sm").Value;

    /// <summary>Gradient track hit area; the checkerboard, gradient bar and stop handles live inside.</summary>
    public static readonly KhamsinClass GradientTrack =
        Cn.Of("relative h-6 w-full cursor-pointer touch-none select-none rounded-full").Value;

    /// <summary>The h-3.5 gradient bar centered inside the taller hit area.</summary>
    public static readonly KhamsinClass GradientBar =
        Cn.Of("pointer-events-none absolute inset-x-0 top-1/2 h-3.5 -translate-y-1/2 overflow-hidden rounded-full").Value;

    /// <summary>Draggable/focusable gradient stop handle; fill color comes from an inline style.</summary>
    public static readonly KhamsinClass StopHandle =
        Cn.Of("absolute top-1/2 h-5 w-5 -translate-x-1/2 -translate-y-1/2 cursor-crosshair rounded-full border-2 border-white shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring").Value;

    public static readonly KhamsinClass StopHandleActive =
        Cn.Of("ring-2 ring-ring").Value;
}
