using Microsoft.AspNetCore.Components;

namespace WaxUI.Alert;

/// <summary>
/// Shows transient or persistent alerts from anywhere in the interactive tree.
/// A <see cref="WaxAlertOutlet"/> placed once in the layout renders the alerts; without
/// an outlet they are queued but never displayed.
/// </summary>
public interface IWaxAlertService
{
    /// <summary>Raised (from any thread) whenever the open-alert list changes.</summary>
    event Action? Changed;

    /// <summary>Currently open alerts, oldest first.</summary>
    IReadOnlyList<WaxAlertInstance> OpenAlerts { get; }

    /// <summary>Shows an alert configured by <paramref name="options"/>.</summary>
    WaxAlertInstance Show(WaxAlertOptions options);

    /// <summary>Shows a transient message with the default duration.</summary>
    WaxAlertInstance Show(
        string message,
        WaxAlertSeverity severity = WaxAlertSeverity.Info,
        TimeSpan? duration = null);

    WaxAlertInstance Success(string message, TimeSpan? duration = null);

    WaxAlertInstance Info(string message, TimeSpan? duration = null);

    WaxAlertInstance Warning(string message, TimeSpan? duration = null);

    /// <summary>Shows a destructive alert (errors).</summary>
    WaxAlertInstance Error(string message, TimeSpan? duration = null);

    /// <summary>Dismisses one alert; false when it was already dismissed.</summary>
    bool Dismiss(WaxAlertInstance alert);

    /// <summary>Dismisses every open alert.</summary>
    void Clear();
}
