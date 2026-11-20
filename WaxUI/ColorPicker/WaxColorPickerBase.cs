using System.Globalization;
using Khamsin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using WaxUI.Services;

namespace WaxUI.ColorPicker;

/// <summary>
/// Shared state and behavior for <see cref="WaxColorPicker"/> (solid colors) and
/// <see cref="WaxGradientColorPicker"/> (CSS gradients): the active color as HSV plus alpha, the
/// readout format, the hex field, presets, open/close state, change emission and JS interop.
/// Subclasses only supply their value shape and their own extra controls.
/// </summary>
public abstract class WaxColorPickerBase : WaxComponent, IAsyncDisposable
{
    protected enum EmitMode
    {
        Change,
        Commit,
    }

    protected readonly record struct Rect(double Left, double Top, double Width, double Height);

    /// <summary>One numeric readout column (R/G/B, H/S/L, …) with its edit callback.</summary>
    protected readonly record struct Channel(string Label, double Value, double Max, Func<double, Task> OnChange, string Suffix = "");

    [Inject] protected IJSRuntime Js { get; set; } = default!;

    // Picker state ---------------------------------------------------------------------------

    private protected WaxColorMath.Rgb _rgb;
    protected double _pickerH;
    protected int _pickerS;
    protected int _pickerV;
    protected int _alpha = 100;

    private WaxColorPickerFormat _format = WaxColorPickerFormat.Hex;
    private bool _open;
    private bool _editing;
    private bool _hexFocused;
    private string _hexInput = string.Empty;
    protected bool _hexInvalid;

    private string? _lastParamValue;
    private string _lastNotified = string.Empty;
    private string _lastSynced = string.Empty;
    private string _lastCommitted = string.Empty;

    private protected WaxJsModule? _jsInterop;
    protected ElementReference _triggerRef;
    protected ElementReference _hexInputRef;

    // Parameters -----------------------------------------------------------------------------

    /// <summary>
    /// Current value: a solid <c>#RRGGBB[AA]</c> hex for <see cref="WaxColorPicker"/> or a CSS
    /// gradient string for <see cref="WaxGradientColorPicker"/>.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>Fires as the value settles: live while typing a full hex, on pointer release for drags.</summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    /// <summary>Fires when the value is committed (pointer up, blur, Enter, preset pick, type change).</summary>
    [Parameter] public EventCallback<string> OnValueCommitted { get; set; }

    /// <summary>Initial value when <see cref="Value"/> is unbound.</summary>
    [Parameter] public string DefaultValue { get; set; } = "#3B82F6";

    /// <summary>Starting alpha percentage (0–100) when the initial color has no alpha channel.</summary>
    [Parameter] public int DefaultAlpha { get; set; } = 100;

    /// <summary>Initial readout format.</summary>
    [Parameter] public WaxColorPickerFormat DefaultFormat { get; set; } = WaxColorPickerFormat.Hex;

    /// <summary>Uncontrolled initial open state for the popover/swatch variants.</summary>
    [Parameter] public bool DefaultOpen { get; set; }

    /// <summary>
    /// Controlled open state for the popover/swatch variants. Only honored when <see cref="OpenChanged"/>
    /// is also bound; otherwise the picker manages its own open state.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Quick-select swatches shown above the saturation field. Entries may be hex colors or CSS
    /// gradient strings (rendered as gradient swatches); unparsable entries are skipped.
    /// </summary>
    [Parameter] public IEnumerable<string>? Presets { get; set; }

    /// <summary>When false, hides alpha controls and keeps the output opaque.</summary>
    [Parameter] public bool ShowAlpha { get; set; } = true;

    /// <summary>Shows a copy-to-clipboard control in the footer row.</summary>
    [Parameter] public bool ShowCopy { get; set; }

    /// <summary>Shows the screen color picker (EyeDropper) control.</summary>
    [Parameter] public bool ShowEyedropper { get; set; } = true;

    /// <summary>Corner style for swatch triggers and preset chips.</summary>
    [Parameter] public WaxColorPickerSwatchShape SwatchShape { get; set; } = WaxColorPickerSwatchShape.Default;

    /// <summary>Inline panel, field popover trigger, or swatch popover trigger.</summary>
    [Parameter] public WaxColorPickerVariant Variant { get; set; } = WaxColorPickerVariant.Inline;

    /// <summary>Hidden input name for native form submission.</summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>Accessible label for the picker group/trigger.</summary>
    [Parameter] public string Label { get; set; } = "Color picker";

    /// <summary>Fires when the EyeDropper API is unavailable in the current browser.</summary>
    [Parameter] public EventCallback OnEyedropperUnsupported { get; set; }

    // Lifecycle ------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        _jsInterop = new WaxJsModule(Js, WaxModulePaths.ColorPicker);

        LoadSource(Value ?? DefaultValue);
        _format = DefaultFormat;
        _open = DefaultOpen;
        _lastParamValue = Value;
        _lastSynced = _lastNotified = _lastCommitted = CurrentValue;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (CoerceOpaque())
        {
            await EmitAsync(EmitMode.Commit);
        }

        // Re-sync from an externally changed Value, unless the change came from us (filtered by
        // _lastSynced) or the user is mid-edit/mid-drag. In that case _lastParamValue is left
        // untouched so the change is re-applied once the interaction ends.
        if (Value is null || Value == _lastParamValue) return;
        if (_editing) return;

        _lastParamValue = Value;
        if (Value == _lastSynced) return;

        LoadSource(Value);
        _lastSynced = _lastNotified = _lastCommitted = CurrentValue;
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsInterop is not null)
        {
            try
            {
                await _jsInterop.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Blazor Server: the circuit is already gone.
            }
        }
    }

    // Value shape (subclass hooks) ------------------------------------------------------------

    /// <summary>The emitted value: a solid hex, or a CSS gradient for the gradient picker.</summary>
    protected abstract string CurrentValue { get; }

    /// <summary>Seeds the picker state from a <see cref="Value"/> or <see cref="DefaultValue"/> string.</summary>
    protected abstract void LoadSource(string source);

    /// <summary>Clears alpha state when <see cref="ShowAlpha"/> is off; true when something changed.</summary>
    protected virtual bool CoerceOpaque()
    {
        if (ShowAlpha || _alpha == 100) return false;

        _alpha = 100;
        return true;
    }

    /// <summary>Applies a preset: solid pickers take its color, gradient pickers may replace the gradient.</summary>
    protected virtual Task SelectPresetAsync(string rawValue) => ApplyHexAsync(rawValue, EmitMode.Commit);

    /// <summary>Pushes active-color edits into any extra state the subclass keeps (gradient stops).</summary>
    protected virtual void SyncActiveColor()
    {
    }

    /// <summary>Value shown (and copied) in the trigger and footer.</summary>
    protected virtual string FormattedValue => Format switch
    {
        WaxColorPickerFormat.Hex => ColorValue,
        WaxColorPickerFormat.Rgb => WaxColorMath.FormatRgb(_rgb, EffectiveAlpha),
        WaxColorPickerFormat.Hsl => WaxColorMath.FormatHsl(_rgb, EffectiveAlpha),
        WaxColorPickerFormat.Oklch => WaxColorMath.FormatOklch(_rgb, EffectiveAlpha),
        _ => throw new ArgumentOutOfRangeException(nameof(Format)),
    };

    /// <summary>Whether the swatch/checkerboard shows a translucent value.</summary>
    protected virtual bool DisplayTranslucent => EffectiveAlpha < 100;

    protected virtual string TriggerSwatchStyle => $"background-color: {ColorValue}";

    // Derived readouts ------------------------------------------------------------------------

    protected bool IsOpen => OpenChanged.HasDelegate ? Open : _open;

    protected bool PanelVisible => Variant == WaxColorPickerVariant.Inline || IsOpen;

    protected WaxColorPickerFormat Format => _format;

    protected int EffectiveAlpha => ShowAlpha ? _alpha : 100;

    protected string ColorValue => WaxColorMath.FormatHex(_rgb, EffectiveAlpha);

    /// <summary>Fully opaque color, for UI that draws alpha separately (alpha track, SV thumb).</summary>
    protected string OpaqueHex => WaxColorMath.FormatHex(_rgb, 100);

    private protected WaxColorMath.Hsl CurrentHsl => WaxColorMath.RgbToHsl(_rgb);

    private protected WaxColorMath.Oklch CurrentOklch => WaxColorMath.RgbToOklch(_rgb);

    protected string HexDisplayValue => _hexFocused ? _hexInput : WaxColorMath.GetDisplayHex(_rgb, EffectiveAlpha, ShowAlpha);

    protected Channel AlphaChannel =>
        new("Alpha", _alpha, 100, value => ApplyAlphaAsync(value, EmitMode.Commit), "%");

    // Color state mutation ---------------------------------------------------------------------

    private protected void SetSolidColor(WaxColorMath.Rgb rgb, int alpha)
    {
        _rgb = rgb;
        _alpha = alpha;
        SyncPickerFromRgb(rgb);
    }

    private protected void SyncPickerFromRgb(WaxColorMath.Rgb rgb, double? hueHint = null)
    {
        var hsv = WaxColorMath.RgbToHsv(rgb);

        var achromatic = hsv.S <= 0 || hsv.V <= 0;
        _pickerH = achromatic ? hueHint ?? _pickerH : hsv.H;
        _pickerS = (int)Math.Clamp(Math.Round(hsv.S), 0, 100);
        _pickerV = (int)Math.Clamp(Math.Round(hsv.V), 0, 100);
    }

    protected void SetHsv(double hue, double saturation, double value)
    {
        _pickerH = WaxColorMath.NormalizeHue(hue);
        _pickerS = (int)Math.Clamp(Math.Round(saturation), 0, 100);
        _pickerV = (int)Math.Clamp(Math.Round(value), 0, 100);
        _rgb = WaxColorMath.HsvToRgb(_pickerH, _pickerS, _pickerV);
        SyncActiveColor();
    }

    protected void SetAlpha(double alpha)
    {
        _alpha = WaxColorMath.ClampPercent(alpha);
        SyncActiveColor();
    }

    protected async Task EmitAsync(EmitMode mode)
    {
        SyncActiveColor();

        var value = CurrentValue;
        if (value != _lastNotified)
        {
            _lastNotified = value;
            _lastSynced = value;
            await ValueChanged.InvokeAsync(value);
        }

        if (mode == EmitMode.Commit && value != _lastCommitted)
        {
            _lastCommitted = value;
            await OnValueCommitted.InvokeAsync(value);
        }
    }

    private protected Task ApplyRgbAsync(WaxColorMath.Rgb rgb, EmitMode mode, double? hueHint = null)
    {
        _rgb = rgb;
        SyncPickerFromRgb(rgb, hueHint);
        return EmitAsync(mode);
    }

    protected Task ApplyHsvAsync(double hue, double saturation, double value, EmitMode mode)
    {
        SetHsv(hue, saturation, value);
        return EmitAsync(mode);
    }

    protected Task ApplyAlphaAsync(double alpha, EmitMode mode)
    {
        if (!ShowAlpha) return Task.CompletedTask;

        SetAlpha(alpha);
        return EmitAsync(mode);
    }

    /// <summary>Applies a hex string; returns false when it cannot be parsed.</summary>
    protected async Task<bool> ApplyHexAsync(string? rawValue, EmitMode mode)
    {
        var parsed = WaxColorMath.ParseHexColor(rawValue);
        if (parsed is null) return false;

        _rgb = parsed.Value.Rgb;
        SyncPickerFromRgb(_rgb);
        _alpha = ShowAlpha ? parsed.Value.AlphaPercent ?? _alpha : 100;
        await EmitAsync(mode);
        return true;
    }

    // Palette ----------------------------------------------------------------------------------

    /// <summary>Marks the picker as being edited so external value changes wait for the interaction to end.</summary>
    protected void SetEditing(bool editing) => _editing = editing;

    /// <summary>Applies a palette move without emitting; the picker notifies when the drag settles.</summary>
    protected Task OnPaletteInputAsync((double Hue, int Saturation, int Value, int Alpha) color)
    {
        SetHsv(color.Hue, color.Saturation, color.Value);
        SetAlpha(color.Alpha);
        return Task.CompletedTask;
    }

    protected Task OnPaletteCommitAsync((double Hue, int Saturation, int Value, int Alpha) color)
    {
        SetHsv(color.Hue, color.Saturation, color.Value);
        SetAlpha(color.Alpha);
        return EmitAsync(EmitMode.Commit);
    }

    // Readout channels -------------------------------------------------------------------------

    /// <summary>Pauses external value sync while a channel input has focus.</summary>
    protected void BeginEdit() => _editing = true;

    protected void EndEdit() => _editing = false;

    private protected Channel RgbChannel(string label, int current, Func<int, WaxColorMath.Rgb> with) =>
        new(label, current, 255, value => ApplyRgbAsync(with(ToByte(value)), EmitMode.Commit));

    protected IEnumerable<Channel> FormatChannels()
    {
        switch (Format)
        {
            case WaxColorPickerFormat.Rgb:
                return
                [
                    RgbChannel("Red", _rgb.R, value => _rgb with { R = value }),
                    RgbChannel("Green", _rgb.G, value => _rgb with { G = value }),
                    RgbChannel("Blue", _rgb.B, value => _rgb with { B = value }),
                ];

            case WaxColorPickerFormat.Hsl:
                var hsl = CurrentHsl;
                return
                [
                    new Channel("Hue", Math.Round(_pickerH), 360, value => SetFromHslAsync(value, CurrentHsl.S, CurrentHsl.L)),
                    new Channel("Saturation", hsl.S, 100, value => SetFromHslAsync(_pickerH, value, CurrentHsl.L)),
                    new Channel("Lightness", hsl.L, 100, value => SetFromHslAsync(_pickerH, CurrentHsl.S, value)),
                ];

            case WaxColorPickerFormat.Oklch:
                var oklch = CurrentOklch;
                return
                [
                    new Channel("Lightness", Math.Round(oklch.L, 2), 1, value => SetFromOklchAsync(value, CurrentOklch.C, CurrentOklch.H)),
                    new Channel("Chroma", Math.Round(oklch.C, 3), WaxColorMath.GetMaxOklchChroma(oklch.L, oklch.H), value => SetFromOklchAsync(CurrentOklch.L, value, CurrentOklch.H)),
                    new Channel("Hue", Math.Round(oklch.H), 360, value => SetFromOklchAsync(CurrentOklch.L, CurrentOklch.C, value)),
                ];

            default:
                return [];
        }
    }

    private Task SetFromHslAsync(double hue, double saturation, double lightness) =>
        ApplyRgbAsync(WaxColorMath.HslToRgb(hue, saturation, lightness), EmitMode.Commit, hueHint: hue);

    private Task SetFromOklchAsync(double lightness, double chroma, double hue) =>
        ApplyRgbAsync(WaxColorMath.OklchToRgb(lightness, chroma, hue), EmitMode.Commit);

    // Hex input --------------------------------------------------------------------------------

    protected void OnHexFocus()
    {
        _editing = true;
        _hexFocused = true;
        _hexInput = WaxColorMath.GetDisplayHex(_rgb, EffectiveAlpha, ShowAlpha);
        _hexInvalid = false;
    }

    protected async Task OnHexInputAsync(ChangeEventArgs e)
    {
        var maxLength = ShowAlpha ? 8 : 6;
        var raw = e.Value?.ToString() ?? string.Empty;
        var sanitized = new string(raw.Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
        if (sanitized.Length > maxLength) sanitized = sanitized[..maxLength];

        _hexInput = sanitized;
        _hexInvalid = false;

        if (sanitized.Length == 6 || (ShowAlpha && sanitized.Length == 8)) await ApplyHexAsync(sanitized, EmitMode.Change);
    }

    protected async Task OnHexBlurAsync()
    {
        _hexFocused = false;
        _editing = false;

        var committed = await ApplyHexAsync(_hexInput, EmitMode.Commit);
        _hexInvalid = !committed && _hexInput.Trim().Length > 0;
    }

    protected async Task OnHexKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && _jsInterop is not null) 
            await _jsInterop.InvokeAsync<object?>("blurElement", _hexInputRef);
    }

    // Footer hooks -----------------------------------------------------------------------------

    protected Task SelectFormatAsync(WaxColorPickerFormat option)
    {
        _format = option;
        return Task.CompletedTask;
    }

    protected Task OnEyedropperPickedAsync(string hex) => ApplyHexAsync(hex, EmitMode.Commit);

    // Open state / keyboard --------------------------------------------------------------------

    protected Task ToggleOpenAsync() => Disabled ? Task.CompletedTask : SetOpenAsync(!IsOpen);

    protected Task CloseOpenAsync() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool value)
    {
        if (!OpenChanged.HasDelegate) _open = value;

        await OpenChanged.InvokeAsync(value);

        // The inline variant has no trigger; focusing its empty reference would throw.
        if (!value && Variant != WaxColorPickerVariant.Inline) await _triggerRef.FocusAsync();
    }

    protected Task OnTriggerKeyDownAsync(KeyboardEventArgs e) =>
        e.Key == "Escape" && IsOpen ? SetOpenAsync(false) : Task.CompletedTask;

    protected Task OnPanelKeyDownAsync(KeyboardEventArgs e) =>
        e.Key == "Escape" && Variant != WaxColorPickerVariant.Inline && IsOpen
            ? SetOpenAsync(false)
            : Task.CompletedTask;

    // Classes ---------------------------------------------------------------------------------

    protected string WrapperClass
    {
        get
        {
            KhamsinClass variantClass = Variant switch
            {
                WaxColorPickerVariant.Inline => Cn.Of("inline-block min-w-0"),
                WaxColorPickerVariant.Popover => Cn.Of("relative inline-flex w-[300px] max-w-full"),
                WaxColorPickerVariant.Swatch => Cn.Of("relative inline-flex"),
                _ => throw new ArgumentOutOfRangeException(nameof(Variant)),
            };

            return Cn.Of(variantClass, Class).Value;
        }
    }

    protected string PanelClass
    {
        get
        {
            KhamsinClass variantClass = Variant switch
            {
                WaxColorPickerVariant.Inline => Cn.Of(WaxColorPickerStyles.Panel, "w-[300px] max-w-full"),
                WaxColorPickerVariant.Popover => Cn.Of(WaxColorPickerStyles.Panel, "absolute left-0 right-0 top-full z-[95] mt-2 w-full"),
                WaxColorPickerVariant.Swatch => Cn.Of(WaxColorPickerStyles.Panel, "absolute left-1/2 top-full z-[95] mt-2 w-[300px] max-w-[calc(100vw_-_2rem)] -translate-x-1/2"),
                _ => throw new ArgumentOutOfRangeException(nameof(Variant)),
            };

            return Cn.Of(variantClass, Disabled ? "pointer-events-none opacity-60" : null).Value;
        }
    }

    protected KhamsinClass TriggerClass => Variant == WaxColorPickerVariant.Popover
        ? WaxColorPickerStyles.PopoverTrigger
        : Cn.Of(WaxColorPickerStyles.SwatchTrigger, ShapeClass).Value;

    protected string TriggerChevronClass => Cn.Of(
        "shrink-0 text-muted-foreground transition-transform duration-200",
        IsOpen ? "rotate-180" : null).Value;

    protected KhamsinClass ShapeClass => Cn.Of(
        SwatchShape == WaxColorPickerSwatchShape.Circle ? "rounded-full" : "rounded-lg");

    protected KhamsinClass CheckerClass => Cn.Of((DisplayTranslucent, WaxColorPickerStyles.AlphaChecker));

    protected string SwatchClass(string size, bool shadow = false) =>
        Cn.Of(WaxColorPickerStyles.Swatch, size, shadow ? "shadow-sm" : null, ShapeClass, CheckerClass).Value;

    protected string TriggerSwatchClass => Variant == WaxColorPickerVariant.Swatch
        ? SwatchClass("h-8 w-8", shadow: true)
        : SwatchClass("h-5 w-5");

    protected string HexInputClass => Cn.Of(
        WaxColorPickerStyles.HexInput,
        _hexInvalid ? "text-destructive" : null).Value;

    // Static helpers --------------------------------------------------------------------------

    /// <summary>Culture-invariant number for inline styles, aria-labels and readouts (server culture may use commas).</summary>
    protected static string FormatNumber(double value) =>
        Math.Round(value, 2).ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Blazor renders a bool attribute as empty/absent; ARIA needs the literal "true"/"false".</summary>
    protected static string Aria(bool value) => value ? "true" : "false";

    protected static int ToByte(double value) => (int)Math.Clamp(Math.Round(value), 0, 255);
}
