namespace WaxUI.ColorPicker;

/// <summary>CSS gradient function emitted by <see cref="WaxColorPicker"/> in gradient mode.</summary>
public enum WaxGradientType
{
    /// <summary><c>linear-gradient({angle}deg, ...)</c> — the angle rotates the stop axis.</summary>
    Linear,

    /// <summary><c>radial-gradient(circle, ...)</c> — always centered, so there is no angle.</summary>
    Radial,

    /// <summary><c>conic-gradient(from {angle}deg, ...)</c> — the angle rotates the start.</summary>
    Conic
}
