namespace WaxUI.Services;

/// <summary>
/// Thread-safe instance list shared by the outlet-driven runtime services. Services add
/// an instance when an overlay opens; the instance's completion task removes it again.
/// <see cref="Changed"/> is raised outside the lock so outlets can marshal the update to
/// their own renderer with <c>InvokeAsync</c>.
/// </summary>
internal sealed class WaxInstanceStore<T> where T : class
{
    private readonly List<T> _items = [];
    private readonly Lock _gate = new();

    public event Action? Changed;

    /// <summary>Snapshot of the open instances, oldest first.</summary>
    public IReadOnlyList<T> Items
    {
        get
        {
            lock (_gate)
            {
                return _items.ToArray();
            }
        }
    }

    public void Add(T item)
    {
        lock (_gate)
        {
            _items.Add(item);
        }

        Changed?.Invoke();
    }

    public bool Remove(T item)
    {
        bool removed;
        lock (_gate)
        {
            removed = _items.Remove(item);
        }

        if (removed)
        {
            Changed?.Invoke();
        }

        return removed;
    }

    public bool Clear()
    {
        lock (_gate)
        {
            if (_items.Count == 0)
            {
                return false;
            }

            _items.Clear();
        }

        Changed?.Invoke();
        return true;
    }
}
