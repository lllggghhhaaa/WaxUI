namespace WaxUI.InputOtp;

/// <summary>
/// Immutable snapshot shared with the slots. A new instance is published whenever the value or
/// focus changes, so cascading subscribers re-render even though the owner component stays the same.
/// </summary>
internal sealed class WaxInputOtpState
{
    public static readonly WaxInputOtpState Empty = new(string.Empty, isFocused: false, activeIndex: -1);

    public WaxInputOtpState(string value, bool isFocused, int activeIndex)
    {
        Value = value;
        IsFocused = isFocused;
        ActiveIndex = activeIndex;
    }

    public string Value { get; }

    public bool IsFocused { get; }

    public int ActiveIndex { get; }

    public char? GetChar(int index)
        => index >= 0 && index < Value.Length ? Value[index] : null;
}
