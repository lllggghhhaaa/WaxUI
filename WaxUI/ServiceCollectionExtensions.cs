using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WaxUI.Alert;
using WaxUI.Dialog;
using WaxUI.Menu;
using WaxUI.Popover;
using WaxUI.Services.Theme;

namespace WaxUI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers WaxUI's scoped runtime services: theme, dialogs, alerts, popovers and
    /// context menus. Each outlet (<c>WaxDialogOutlet</c>, <c>WaxAlertOutlet</c>,
    /// <c>WaxPopoverOutlet</c>, <c>WaxMenuOutlet</c>) renders what its service opens.
    /// </summary>
    public static IServiceCollection AddWaxUI(this IServiceCollection services)
    {
        services.TryAddScoped<IThemeService, ThemeService>();
        services.TryAddScoped<IWaxDialogService, WaxDialogService>();
        services.TryAddScoped<IWaxAlertService, WaxAlertService>();
        services.TryAddScoped<IWaxPopoverService, WaxPopoverService>();
        services.TryAddScoped<IWaxMenuService, WaxMenuService>();

        return services;
    }
}
