namespace WaxUI.Services;

/// <summary>
/// Static asset paths of the generated browser modules in <c>wwwroot/js</c>. Kept in one
/// place so every runtime feature imports its module through the same convention.
/// </summary>
internal static class WaxModulePaths
{
    public const string ColorPicker = "./_content/WaxUI/js/colorPicker.g.js";
    public const string Dialog = "./_content/WaxUI/js/dialog.g.js";
    public const string Dropzone = "./_content/WaxUI/js/dropzone.g.js";
    public const string Popover = "./_content/WaxUI/js/popover.g.js";
    public const string Theme = "./_content/WaxUI/js/theme.g.js";
}
