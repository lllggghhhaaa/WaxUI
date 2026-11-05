using Microsoft.AspNetCore.Components.Routing;

namespace WaxUI.Navigation;

public record WaxSidebarItem(
    string Href,
    string Label,
    string? Icon = null,
    NavLinkMatch Match = NavLinkMatch.Prefix
);