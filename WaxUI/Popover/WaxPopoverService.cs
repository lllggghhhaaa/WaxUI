using Microsoft.AspNetCore.Components;
using WaxUI.Services;

namespace WaxUI.Popover;

/// <summary>
/// Default <see cref="IWaxPopoverService"/>: keeps the open popovers and removes each
/// instance once it closes. Registered scoped (one stack per circuit/user flow) by
/// AddWaxUI.
/// </summary>
public sealed class WaxPopoverService : IWaxPopoverService
{
    private readonly WaxInstanceStore<WaxPopoverInstance> _popovers = new();

    public event Action? Changed
    {
        add => _popovers.Changed += value;
        remove => _popovers.Changed -= value;
    }

    public IReadOnlyList<WaxPopoverInstance> OpenPopovers => _popovers.Items;

    public Task<IWaxPopoverHandle> ShowAsync(
        ElementReference anchor,
        RenderFragment content,
        WaxPopoverOptions? options = null) =>
        Task.FromResult(Add(anchor, content, options));

    public Task<IWaxPopoverHandle> ShowAsync(
        string anchorSelector,
        RenderFragment content,
        WaxPopoverOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorSelector);
        return Task.FromResult(Add(anchorSelector, content, options));
    }

    public bool Close(IWaxPopoverHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);

        if (handle is not WaxPopoverInstance instance || !instance.IsOpen)
        {
            return false;
        }

        _ = instance.CloseAsync();
        return true;
    }

    public void CloseAll()
    {
        foreach (var popover in _popovers.Items)
        {
            _ = popover.CloseAsync();
        }
    }

    private IWaxPopoverHandle Add(object anchor, RenderFragment content, WaxPopoverOptions? options)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(content);

        var instance = new WaxPopoverInstance(anchor, content, options ?? new WaxPopoverOptions());

        _popovers.Add(instance);
        _ = RemoveWhenClosedAsync(instance);

        return instance;
    }

    private async Task RemoveWhenClosedAsync(WaxPopoverInstance instance)
    {
        await instance.Closed;
        _popovers.Remove(instance);
    }
}
