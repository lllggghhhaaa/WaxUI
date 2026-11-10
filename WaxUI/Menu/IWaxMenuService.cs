using Microsoft.AspNetCore.Components.Web;

namespace WaxUI.Menu;

/// <summary>
/// Opens context menus imperatively and awaits the selected row index, or null when
/// dismissed. A <see cref="WaxMenuOutlet"/> placed once in the interactive layout/page
/// renders whatever the service has open; without an outlet nothing is displayed.
/// </summary>
public interface IWaxMenuService
{
    /// <summary>Raised (from any thread) whenever the open-menu list changes.</summary>
    event Action? Changed;

    /// <summary>Currently open menus, oldest first.</summary>
    IReadOnlyList<WaxMenuInstance> OpenMenus { get; }

    /// <summary>Opens a menu at viewport coordinates.</summary>
    Task<int?> ShowAsync(WaxMenuOptions options, double x, double y);

    /// <summary>Opens a headerless menu at viewport coordinates.</summary>
    Task<int?> ShowAsync(IReadOnlyList<WaxMenuItem> items, double x, double y);

    /// <summary>Opens a menu at the pointer position carried by a mouse event.</summary>
    Task<int?> ShowAsync(WaxMenuOptions options, MouseEventArgs args);

    /// <summary>Dismisses one menu; false when it was already resolved.</summary>
    bool Close(WaxMenuInstance menu);

    /// <summary>Dismisses every open menu.</summary>
    void CloseAll();
}
