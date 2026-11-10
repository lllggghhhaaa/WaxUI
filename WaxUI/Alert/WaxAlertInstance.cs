namespace WaxUI.Alert;

/// <summary>One alert shown by <see cref="WaxAlertOutlet"/>.</summary>
public sealed class WaxAlertInstance
{
    private readonly TaskCompletionSource _dismissed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal WaxAlertInstance(WaxAlertOptions options) => Options = options;

    public Guid Id { get; } = Guid.NewGuid();

    public WaxAlertOptions Options { get; }

    public bool IsDismissed => _dismissed.Task.IsCompleted;

    /// <summary>Completes when the alert is dismissed or auto-dismissed.</summary>
    internal Task Dismissed => _dismissed.Task;

    /// <summary>Dismisses this alert. Idempotent.</summary>
    public void Dismiss() => _dismissed.TrySetResult();
}
