namespace WaxUI.Menu;

/// <summary>One context menu shown by <see cref="WaxMenuOutlet"/>.</summary>
public sealed class WaxMenuInstance
{
    private readonly TaskCompletionSource<int?> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal WaxMenuInstance(WaxMenuOptions options, double x, double y)
    {
        Options = options;
        X = x;
        Y = y;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public WaxMenuOptions Options { get; }

    public double X { get; }

    public double Y { get; }

    public bool IsOpen => !_completion.Task.IsCompleted;

    /// <summary>The selected row index, or null when the menu was dismissed.</summary>
    public Task<int?> Result => _completion.Task;

    /// <summary>Resolves the menu with the chosen row. Idempotent.</summary>
    public bool Select(int index) => _completion.TrySetResult(index);

    /// <summary>Resolves the menu as dismissed. Idempotent.</summary>
    public bool Dismiss() => _completion.TrySetResult(null);
}
