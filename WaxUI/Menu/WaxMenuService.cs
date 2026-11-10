using Microsoft.AspNetCore.Components.Web;
using WaxUI.Services;

namespace WaxUI.Menu;

/// <summary>
/// Default <see cref="IWaxMenuService"/>: keeps the open menus and removes each instance
/// once it resolves. Registered scoped (one stack per circuit/user flow) by AddWaxUI.
/// </summary>
public sealed class WaxMenuService : IWaxMenuService
{
    private readonly WaxInstanceStore<WaxMenuInstance> _menus = new();

    public event Action? Changed
    {
        add => _menus.Changed += value;
        remove => _menus.Changed -= value;
    }

    public IReadOnlyList<WaxMenuInstance> OpenMenus => _menus.Items;

    public Task<int?> ShowAsync(WaxMenuOptions options, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(options);

        var instance = new WaxMenuInstance(options, x, y);

        _menus.Add(instance);
        _ = RemoveWhenResolvedAsync(instance);

        return instance.Result;
    }

    public Task<int?> ShowAsync(IReadOnlyList<WaxMenuItem> items, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(items);
        return ShowAsync(new WaxMenuOptions { Items = items }, x, y);
    }

    public Task<int?> ShowAsync(WaxMenuOptions options, MouseEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return ShowAsync(options, args.ClientX, args.ClientY);
    }

    public bool Close(WaxMenuInstance menu)
    {
        ArgumentNullException.ThrowIfNull(menu);

        if (!menu.IsOpen)
        {
            return false;
        }

        return menu.Dismiss();
    }

    public void CloseAll()
    {
        foreach (var menu in _menus.Items)
        {
            menu.Dismiss();
        }
    }

    private async Task RemoveWhenResolvedAsync(WaxMenuInstance instance)
    {
        await instance.Result;
        _menus.Remove(instance);
    }
}
