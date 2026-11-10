using Microsoft.AspNetCore.Components;

namespace WaxUI.Alert;

/// <summary>
/// Options for a service-driven alert. The alert is rendered by
/// <see cref="WaxAlertOutlet"/> using <see cref="WaxAlert"/>; the outlet adds nothing
/// but stacking, positioning and the optional dismiss button.
/// </summary>
public sealed record WaxAlertOptions
{
    /// <summary>Default lifetime of a transient alert (five seconds).</summary>
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(5);

    /// <summary>Primary message; may be empty when only <see cref="ChildContent"/> is given.</summary>
    public string Message { get; init; } = string.Empty;

    public WaxAlertSeverity Severity { get; init; } = WaxAlertSeverity.Info;

    /// <summary>Overrides the default severity icon with a custom Lucide icon name.</summary>
    public string? Icon { get; init; }

    /// <summary>Optional body/actions rendered under the message (e.g. a retry button).</summary>
    public RenderFragment? ChildContent { get; init; }

    /// <summary>Hides the message behind a blur + gradient veil, revealed on click.</summary>
    public bool Spoiler { get; init; }

    /// <summary>Shows a dismiss button in the alert's top-right corner.</summary>
    public bool Dismissible { get; init; } = true;

    /// <summary>
    /// How long the alert stays open. <c>null</c> keeps it open until dismissed.
    /// <see cref="TimeSpan.Zero"/> or a negative value also disables auto-dismiss.
    /// </summary>
    public TimeSpan? Duration { get; init; } = DefaultDuration;

    /// <summary>Extra classes merged onto the underlying <see cref="WaxAlert"/>.</summary>
    public string Class { get; init; } = string.Empty;
}
