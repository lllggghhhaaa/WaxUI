using Khamsin;

namespace WaxUI.Popover;

/// <summary>
/// Shared Khamsin utility classes for WaxPopover and the service-driven popover host so
/// both render exactly the same panel and arrow.
/// </summary>
internal static class WaxPopoverStyles
{
    public static readonly KhamsinClass Panel =
        Cn.Of("fixed z-[96] max-w-[calc(100vw_-_1rem)] rounded-3xl border border-border bg-popover text-popover-foreground shadow-2xl shadow-black/15");

    public static readonly KhamsinClass Enter =
        Cn.Of("animate-in fade-in-0 zoom-in-95 duration-150");

    public static string Arrow(string? appliedPlacement)
    {
        KhamsinClass borders = appliedPlacement switch
        {
            "top-start" or "top" or "top-end" => Cn.Of("border-b border-r"),
            "left-start" or "left" or "left-end" => Cn.Of("border-t border-r"),
            "right-start" or "right" or "right-end" => Cn.Of("border-l border-b"),
            _ => Cn.Of("border-t border-l"),
        };

        return Cn.Of("absolute size-2 rotate-45 bg-popover", borders).Value;
    }
}
