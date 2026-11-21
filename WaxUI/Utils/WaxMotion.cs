namespace WaxUI.Utils;

/// <summary>
/// Shared motion vocabulary: durations and entrance/exit class strings used across the
/// library, so every overlay moves on the same curves and timings. Durations stick to
/// Khamsin's built-in <c>duration-*</c> scale — 150ms fast (panels, hovers, marks),
/// 200ms base (buttons, dialog enter), 300ms slow (accordion, switch, card) — and the
/// curves come from the <c>ease-emphasized</c> / <c>ease-emphasized-accelerate</c>
/// tokens defined in khamsin.json. <c>prefers-reduced-motion</c> collapses all of this
/// centrally in app.css; components never need their own guard.
/// </summary>
public static class WaxMotion
{
    /// <summary>Fast interactions: panel entrances, dropdown rows, check marks (150ms).</summary>
    public const int FastMs = 150;

    /// <summary>Base transitions: buttons, dialog enter, floating labels (200ms).</summary>
    public const int BaseMs = 200;

    /// <summary>Slow transitions: accordion bodies, switch travel, card lift (300ms).</summary>
    public const int SlowMs = 300;

    /// <summary>
    /// Exit-animation length in ms. Services and JS wait this long (or for the animation
    /// itself to finish) before unmounting/closing, so exits can play.
    /// </summary>
    public const int ExitMs = 150;

    /// <summary>
    /// Generic floating-panel entrance: fade + slight zoom. Use for panels that are not
    /// anchored below their trigger (context menus, centered popovers).
    /// </summary>
    public const string PanelEnter = "animate-in fade-in-0 zoom-in-95 duration-150 ease-emphasized";

    /// <summary>
    /// Anchored-dropdown entrance: <see cref="PanelEnter"/> plus a small drop from the
    /// trigger edge. Use for Select/DatePicker/ColorPicker panels that open beneath (or
    /// above) their trigger.
    /// </summary>
    public const string DropdownEnter = PanelEnter + " slide-in-from-top-2";
}
