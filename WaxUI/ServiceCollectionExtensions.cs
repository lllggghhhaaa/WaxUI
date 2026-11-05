using Microsoft.Extensions.DependencyInjection;
using WaxUI.Dialog;
using WaxUI.Services.Theme;

namespace WaxUI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWaxUI(this IServiceCollection services)
    {
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IWaxDialogService, WaxDialogService>();

        return services;
    }
}