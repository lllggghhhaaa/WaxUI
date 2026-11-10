# WaxUI

A UI component library for Blazor, styled with Khamsin utility CSS and light/dark mode support.

- NuGet: [https://www.nuget.org/packages/WaxUI](https://www.nuget.org/packages/WaxUI)
- Source: [https://github.com/lllggghhhaaa/WaxUI](https://github.com/lllggghhhaaa/WaxUI)
- License: Apache-2.0
- Target: .NET 11.0

Components: Accordion, Alert, Avatar, Badge, Breadcrumb, Button, Card, Checkbox, Chip, Channel Button, Color Picker, Date Picker, Dialog, Drop Overlay, Dropzone, Empty State, Fab, Icon Button, Navigation (Sidebar, Server Button), Popover, Progress, QR Code, Radio, Reaction Chip, Section Heading, Select, Setting Row, Slider, Spoiler, Switch, Tabs, Text Field.

## Install

```bash
dotnet add package WaxUI
```

## Setup

Register the services in `Program.cs`:

```csharp
builder.Services.AddWaxUI();
```

Add Khamsin to projects that write utility classes, then link the generated stylesheet and theme
support in `App.razor`:

```bash
dotnet add package Khamsin --version 0.4.0
```

```razor
<link rel="stylesheet" href="khamsin.css" />
<FontMap />
<ThemePreloader />
```

`khamsin.css` contains the theme, Khamsin's preflight, WaxUI's base styles and only the
utilities the app renders. Blazor Server apps can serve it from an endpoint instead with
`app.MapKhamsinCss()`.

## Usage

```razor
<WaxButton Variant="WaxButtonVariant.Filled" OnClick="OnClick">
    <ChildContent>Click me</ChildContent>
</WaxButton>

<WaxBadge Color="WaxColor.Primary">New</WaxBadge>

<WaxSwitch Label="Dark mode" />
```

```csharp
private void OnClick() { }
```

### Custom colors

Components that accept a `WaxColor` (Badge, Progress Bar, Slider) also accept custom theme tokens and explicit CSS colors:

```razor
@code {
    // Resolved as var(--brand); define --brand in your theme.
    private static readonly WaxColor Brand = new("brand");
    // Any CSS color value works.
    private static readonly WaxColor BrandHex = WaxColor.Custom("#7B5EA7");
}

<WaxBadge Color="Brand">Brand</WaxBadge>
<WaxBadge Color="BrandHex">Custom hex</WaxBadge>
```

Filled custom tokens use `--{token}-foreground` when defined; otherwise white is used for the text.

## Theming

Inject `IThemeService` to change the theme at runtime:

```razor
@inject IThemeService Theme

<button @onclick="() => Theme.ToggleDarkLightAsync()">Toggle theme</button>
```

Modes: `ThemeMode.System`, `ThemeMode.Light`, `ThemeMode.Dark`.

## Runtime services

`AddWaxUI()` registers scoped services for UI that lives outside the component that
opens it. The overlay services need their outlet rendered once in the interactive
layout; `ThemePreloader` belongs in the document head.

| Service | Host | Purpose |
| --- | --- | --- |
| `IThemeService` | `ThemePreloader` | Read/toggle the current light/dark/system theme. |
| `IWaxDialogService` | `WaxDialogOutlet` | Await typed dialog, confirm and prompt results. |
| `IWaxAlertService` | `WaxAlertOutlet` | Transient or persistent stacked alerts/toasts. |
| `IWaxPopoverService` | `WaxPopoverOutlet` | Anchored popovers opened from code or a selector. |
| `IWaxMenuService` | `WaxMenuOutlet` | Context menus that complete with the selected row. |

```razor
<WaxDialogOutlet @rendermode="InteractiveServer" />
<WaxAlertOutlet @rendermode="InteractiveServer" />
<WaxPopoverOutlet @rendermode="InteractiveServer" />
<WaxMenuOutlet @rendermode="InteractiveServer" />
```

```csharp
@inject IWaxAlertService Alerts

Alerts.Success("Your changes have been saved.");
Alerts.Error("The upload failed. Please try again.");
```

```csharp
@inject IWaxPopoverService Popovers

private ElementReference _anchor;

private Task OpenAsync() => Popovers.ShowAsync(
    _anchor,
    @<div class="w-64 p-3">Anchored content</div>,
    new WaxPopoverOptions { ShowArrow = true });
```

```csharp
@inject IWaxMenuService Menus

private async Task OpenMenuAsync(MouseEventArgs e)
{
    var index = await Menus.ShowAsync(new WaxMenuOptions
    {
        HeaderLabel = "design-team",
        HeaderKind = "channel",
        Items = [new("Copy link", "link"), new("Delete", "trash-2", danger: true)],
    }, e);

    // index: selected row, or null when dismissed
}
```
