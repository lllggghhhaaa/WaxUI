using Khamsin;
using WaxUI.Utils;

namespace WaxUI.Alert;

/// <summary>
/// Entrance/exit classes for <see cref="WaxAlertOutlet"/> wrappers, written as literals so
/// Khamsin extracts them; the outlet only picks between them at runtime.
/// </summary>
internal static class WaxAlertStyles
{
    public static readonly KhamsinClass EnterTop =
        Cn.Of("animate-in fade-in-0 duration-200 ease-emphasized slide-in-from-top-2");

    public static readonly KhamsinClass EnterBottom =
        Cn.Of("animate-in fade-in-0 duration-200 ease-emphasized slide-in-from-bottom-2");

    public static readonly KhamsinClass ExitTop =
        Cn.Of("animate-out fade-out-0 duration-150 ease-emphasized-accelerate slide-out-to-top-2");

    public static readonly KhamsinClass ExitBottom =
        Cn.Of("animate-out fade-out-0 duration-150 ease-emphasized-accelerate slide-out-to-bottom-2");
}
