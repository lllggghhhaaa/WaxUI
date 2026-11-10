namespace WaxUI.Popover;

public static class WaxPopoverPlacementExtensions
{
    /// <summary>Converts the enum to the kebab-case token used by the popover JS module.</summary>
    public static string ToPlacementName(this WaxPopoverPlacement placement) => placement switch
    {
        WaxPopoverPlacement.TopStart => "top-start",
        WaxPopoverPlacement.Top => "top",
        WaxPopoverPlacement.TopEnd => "top-end",
        WaxPopoverPlacement.BottomStart => "bottom-start",
        WaxPopoverPlacement.Bottom => "bottom",
        WaxPopoverPlacement.BottomEnd => "bottom-end",
        WaxPopoverPlacement.LeftStart => "left-start",
        WaxPopoverPlacement.Left => "left",
        WaxPopoverPlacement.LeftEnd => "left-end",
        WaxPopoverPlacement.RightStart => "right-start",
        WaxPopoverPlacement.Right => "right",
        WaxPopoverPlacement.RightEnd => "right-end",
        _ => "bottom-start",
    };
}
