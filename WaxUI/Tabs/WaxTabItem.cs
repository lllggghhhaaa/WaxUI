using Microsoft.AspNetCore.Components;

namespace WaxUI.Tabs;

public record WaxTabItem(string? Label, RenderFragment Content, string? Icon = null)
{
    public bool Disabled { get; init; }

    public RenderFragment? IconContent { get; init; }
}
