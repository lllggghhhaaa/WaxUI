using Khamsin;

namespace WaxUI.Spoiler;

/// <summary>
/// Shared Khamsin utility classes for the spoiler (blur + gradient veil) effect.
/// Centralized so WaxSpoiler, WaxChip, WaxReactionChip and WaxSettingRow render
/// identical visuals; the rose veil colors come from the --spoiler-* tokens in
/// Theme/tokens.css. The bare <c>wax-spoiler</c>/<c>wax-spoiler-veil</c> marker
/// classes carry no styles and exist only as styling/testing hooks.
/// </summary>
internal static class WaxSpoilerStyles
{
    /// <summary>Wrapper: positions the veil, groups hover state and rounds the focus ring.</summary>
    public static readonly KhamsinClass Wrapper =
        Khamsin.Cn.Of(
            "relative cursor-pointer rounded-[calc(var(--radius)_-_8px)] "
            + "focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring").Value
        + " wax-spoiler group/wax-spoiler";

    /// <summary>Hidden state: keeps the blurred content from being selected or copied.</summary>
    public static readonly KhamsinClass Hidden = Khamsin.Cn.Of("select-none").Value;

    /// <summary>Content: animates the blur away when revealing.</summary>
    public static readonly KhamsinClass Content = Khamsin.Cn.Of("duration-[250ms]").Value + " wax-transition-filter";

    /// <summary>Hidden state content: the blur itself.</summary>
    public static readonly KhamsinClass ContentBlurred = Khamsin.Cn.Of("blur-[5px]").Value;

    /// <summary>Gradient veil overlay; fades out when revealed.</summary>
    public static readonly KhamsinClass Veil =
        Khamsin.Cn.Of(
            "absolute inset-0 flex items-center justify-center pointer-events-none rounded-[inherit] "
            + "transition-opacity duration-[250ms] "
            + "bg-[linear-gradient(to_right,transparent_0%,var(--spoiler-veil-from)_35%,var(--spoiler-veil-to)_100%)]").Value
        + " wax-spoiler-veil";

    /// <summary>Hidden state veil: visible.</summary>
    public static readonly KhamsinClass VeilVisible = Khamsin.Cn.Of("opacity-100").Value;

    /// <summary>Revealed state veil: faded out.</summary>
    public static readonly KhamsinClass VeilHidden = Khamsin.Cn.Of("opacity-0").Value;

    /// <summary>Eye hint shown while hidden; brightens on wrapper hover.</summary>
    public static readonly KhamsinClass Hint = Khamsin.Cn.Of(
        "opacity-75 transition-all duration-[200ms] text-(--spoiler-hint) "
        + "group-hover/wax-spoiler:opacity-100 group-hover/wax-spoiler:text-foreground").Value;
}
