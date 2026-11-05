namespace WaxUI.Theme;

/// <summary>
/// A color reference used by Wax components. By default <see cref="Name"/> is a theme
/// token resolved through its CSS variable (<c>var(--{Name})</c>); use
/// <see cref="Custom"/> for arbitrary CSS colors such as hex or rgb values.
/// </summary>
public record WaxColor(string Name)
{
    public static WaxColor Primary => new("primary");
    public static WaxColor Secondary => new("secondary");
    public static WaxColor Accent => new("accent");
    public static WaxColor Muted => new("muted");

    /// <summary>
    /// Explicit CSS color value, e.g. <c>#7B5EA7</c> or <c>rgb(123 94 167)</c>.
    /// When null, <see cref="Name"/> is treated as a theme token.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>True when this color references a theme token rather than an explicit CSS value.</summary>
    public bool IsToken => Value is null;

    /// <summary>CSS color value for inline styles, resolved from the token or the explicit value.</summary>
    public string CssValue => Value ?? $"var(--{Name})";

    /// <summary>
    /// Creates a color from an arbitrary CSS color value, e.g.
    /// <c>WaxColor.Custom("#7B5EA7")</c>.
    /// </summary>
    public static WaxColor Custom(string cssValue)
    {
        var value = cssValue.Trim();
        return new WaxColor(value) { Value = value };
    }

    /// <summary>
    /// Creates a color from a theme token name, e.g. <c>WaxColor.Token("brand")</c> for a
    /// custom <c>--brand</c> variable. Same as constructing <c>new WaxColor("brand")</c>.
    /// </summary>
    public static WaxColor Token(string name)
    {
        var token = name.Trim();
        return new WaxColor(token.StartsWith("--", StringComparison.Ordinal) ? token[2..] : token);
    }

    public override string ToString() => Value ?? Name;
}
