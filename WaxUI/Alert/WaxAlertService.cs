using WaxUI.Services;

namespace WaxUI.Alert;

/// <summary>
/// Default <see cref="IWaxAlertService"/>: keeps the open alerts, schedules
/// auto-dismissal and removes instances once they are dismissed. Registered scoped
/// (one queue per circuit/user flow) by AddWaxUI.
/// </summary>
public sealed class WaxAlertService : IWaxAlertService
{
    private readonly WaxInstanceStore<WaxAlertInstance> _alerts = new();

    public event Action? Changed
    {
        add => _alerts.Changed += value;
        remove => _alerts.Changed -= value;
    }

    public IReadOnlyList<WaxAlertInstance> OpenAlerts => _alerts.Items;

    public WaxAlertInstance Show(WaxAlertOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var alert = new WaxAlertInstance(options);

        _alerts.Add(alert);
        _ = RemoveWhenDismissedAsync(alert);

        if (options.Duration is { } duration && duration > TimeSpan.Zero)
        {
            _ = DismissAfterAsync(alert, duration);
        }

        return alert;
    }

    public WaxAlertInstance Show(
        string message,
        WaxAlertSeverity severity = WaxAlertSeverity.Info,
        TimeSpan? duration = null)
    {
        var options = new WaxAlertOptions
        {
            Message = message,
            Severity = severity,
        };

        if (duration is { } value)
        {
            options = options with { Duration = value };
        }

        return Show(options);
    }

    public WaxAlertInstance Success(string message, TimeSpan? duration = null) =>
        Show(message, WaxAlertSeverity.Success, duration);

    public WaxAlertInstance Info(string message, TimeSpan? duration = null) =>
        Show(message, WaxAlertSeverity.Info, duration);

    public WaxAlertInstance Warning(string message, TimeSpan? duration = null) =>
        Show(message, WaxAlertSeverity.Warning, duration);

    public WaxAlertInstance Error(string message, TimeSpan? duration = null) =>
        Show(message, WaxAlertSeverity.Destructive, duration);

    public bool Dismiss(WaxAlertInstance alert)
    {
        ArgumentNullException.ThrowIfNull(alert);

        if (alert.IsDismissed)
        {
            return false;
        }

        alert.Dismiss();
        return true;
    }

    public void Clear()
    {
        var alerts = _alerts.Items;
        if (alerts.Count == 0)
        {
            return;
        }

        _alerts.Clear();

        foreach (var alert in alerts)
        {
            alert.Dismiss();
        }
    }

    private async Task RemoveWhenDismissedAsync(WaxAlertInstance alert)
    {
        await alert.Dismissed;
        _alerts.Remove(alert);
    }

    private async Task DismissAfterAsync(WaxAlertInstance alert, TimeSpan duration)
    {
        var delay = Task.Delay(duration);

        if (await Task.WhenAny(delay, alert.Dismissed) == delay)
        {
            alert.Dismiss();
        }
    }
}
