using System.Globalization;

namespace WaxUI.ColorPicker;

/// <summary>
/// CSS gradient parsing/formatting helpers used by <see cref="WaxGradientColorPicker"/>.
/// Handles exactly what the picker emits plus tolerant reads of common authoring forms
/// (<c>deg</c>, <c>from Ndeg</c>, <c>to &lt;side&gt;</c>, <c>circle</c>/<c>ellipse</c>,
/// <c>rgb()/rgba()</c> and hex stops, optional percents). Everything is culture-invariant;
/// anything unparsable makes the whole value null so callers can fall back, mirroring
/// <see cref="WaxColorMath.ParseHexColor"/> semantics.
/// </summary>
internal static class WaxGradientMath
{
    private const MidpointRounding Rounding = MidpointRounding.AwayFromZero;

    /// <summary>One color stop: opaque or translucent color plus its 0–100 track position.</summary>
    internal readonly record struct GradientStop(WaxColorMath.Rgb Rgb, int AlphaPercent, double Position);

    internal readonly record struct ParsedGradient(WaxGradientType Type, double Angle, List<GradientStop> Stops);

    private static readonly string[] LinearPrefixes = ["linear-gradient("];
    private static readonly string[] RadialPrefixes = ["radial-gradient("];
    private static readonly string[] ConicPrefixes = ["conic-gradient("];

    #region Parsing

    /// <summary>Parses a linear/radial/conic CSS gradient string; null when it is not a gradient we understand.</summary>
    public static ParsedGradient? ParseGradient(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var raw = input.Trim().ToLowerInvariant();
        var prefix = LinearPrefixes.Concat(RadialPrefixes).Concat(ConicPrefixes)
            .FirstOrDefault(raw.StartsWith) ?? "";

        var type = prefix switch
        {
            "linear-gradient(" => WaxGradientType.Linear,
            "radial-gradient(" => WaxGradientType.Radial,
            "conic-gradient(" => WaxGradientType.Conic,
            _ => (WaxGradientType)(-1)
        };
        if (type < 0)
            return null;

        var bodyArgs = ExtractBalancedBody(raw, prefix.Length);
        if (bodyArgs is null)
            return null;

        var args = SplitTopLevel(bodyArgs);
        if (args is null)
            return null;

        double angle = type switch
        {
            WaxGradientType.Conic => 0,
            WaxGradientType.Radial => 0,
            _ => 180
        };

        var stops = new List<(WaxColorMath.Rgb Rgb, int Alpha, double? Position)>();
        foreach (var arg in args)
        {
            if (TryParseDescriptor(arg, type, ref angle))
                continue;

            var stop = ParseStopArgument(arg);
            if (stop is null)
                return null;

            stops.Add(stop.Value);
        }

        if (stops.Count < 2)
            return null;

        return new ParsedGradient(type, angle, DistributePositions(stops));
    }

    /// <summary>Extracts the text between the opening parenthesis and its balanced closing one.</summary>
    private static string? ExtractBalancedBody(string value, int start)
    {
        var depth = 1;
        for (var i = start; i < value.Length; i++)
        {
            if (value[i] == '(')
                depth++;
            else if (value[i] == ')')
            {
                depth--;
                if (depth == 0)
                    return value[start..i];
            }
        }

        return null;
    }

    /// <summary>Splits on commas that sit outside any parentheses.</summary>
    private static string[]? SplitTopLevel(string body)
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;

        for (var i = 0; i < body.Length; i++)
        {
            switch (body[i])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case ',' when depth == 0:
                    parts.Add(body[start..i]);
                    start = i + 1;
                    break;
            }
        }

        parts.Add(body[start..]);
        return parts.Count == 0 ? null : parts.ToArray();
    }

    /// <summary>Consumes angle/shape descriptors; returns false when the argument must be a color stop.</summary>
    private static bool TryParseDescriptor(string arg, WaxGradientType type, ref double angle)
    {
        var token = arg.Trim();
        if (token.Length == 0)
            return false;

        // "from 45deg" (conic) or a bare "45deg" (linear).
        var degToken = token.StartsWith("from ") ? token[5..].Trim() : token;
        if (degToken.EndsWith("deg") &&
            double.TryParse(degToken[..^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var degrees))
        {
            angle = WaxColorMath.NormalizeHue(degrees);
            return true;
        }

        // "to right", "to bottom left" and friends.
        if (token.StartsWith("to "))
        {
            angle = token[3..].Trim() switch
            {
                "top" => 0,
                "top right" or "right top" => 45,
                "right" => 90,
                "bottom right" or "right bottom" => 135,
                "bottom" => 180,
                "bottom left" or "left bottom" => 225,
                "left" => 270,
                "top left" or "left top" => 315,
                _ => angle,
            };
            return true;
        }

        // Radial/conic shape and center keywords are accepted and ignored ("circle", "at 30% 40%", ...).
        if (type is WaxGradientType.Radial or WaxGradientType.Conic &&
            (token is "circle" or "ellipse" || token.StartsWith("at ") || token.Contains("closest") || token.Contains("farthest")))
        {
            return true;
        }

        return false;
    }

    /// <summary>Parses one "color [position]" argument, e.g. "#f00 10%" or "rgba(255,0,0,.5)".</summary>
    private static (WaxColorMath.Rgb Rgb, int Alpha, double? Position)? ParseStopArgument(string arg)
    {
        var token = arg.Trim();
        if (token.Length == 0)
            return null;

        string colorPart;
        string? rest;

        if (token.StartsWith("rgb(") || token.StartsWith("rgba("))
        {
            var openIndex = token.IndexOf('(');
            var bodyEnd = token.IndexOf(')', openIndex);
            if (bodyEnd < 0)
                return null;

            colorPart = token[..(bodyEnd + 1)];
            rest = token[(bodyEnd + 1)..].Trim();
        }
        else
        {
            var split = token.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            colorPart = split[0];
            rest = split.Length > 1 ? string.Concat(split[1..]) : null;
        }

        var color = ParseColor(colorPart);
        if (color is null)
            return null;

        double? position = null;
        if (rest is { Length: > 0 })
        {
            if (!rest.EndsWith('%') ||
                !double.TryParse(rest[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var percent))
                return null;

            position = Math.Clamp(percent, 0, 100);
        }

        return (color.Value.Rgb, color.Value.AlphaPercent ?? 100, position);
    }

    /// <summary>Hex (3/4/6/8 digits) or comma-syntax rgb()/rgba(); null when unrecognized.</summary>
    private static WaxColorMath.ParsedHexColor? ParseColor(string input)
    {
        if (input.StartsWith('#') || (input.Length is 3 or 4 or 6 or 8 && input.All(char.IsAsciiHexDigit)))
            return WaxColorMath.ParseHexColor(input);

        if (!input.StartsWith("rgb"))
            return null;

        var openIndex = input.IndexOf('(');
        var closeIndex = input.IndexOf(')', openIndex);
        if (openIndex < 0 || closeIndex < 0)
            return null;

        var channels = input[(openIndex + 1)..closeIndex].Split(',');
        if (channels.Length is not (3 or 4))
            return null;

        var numbers = new double[channels.Length];
        for (var i = 0; i < channels.Length; i++)
        {
            if (!double.TryParse(channels[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[i]))
                return null;
        }

        var rgb = new WaxColorMath.Rgb(
            (int)Math.Clamp(Math.Round(numbers[0], Rounding), 0, 255),
            (int)Math.Clamp(Math.Round(numbers[1], Rounding), 0, 255),
            (int)Math.Clamp(Math.Round(numbers[2], Rounding), 0, 255));

        var alpha = 100;
        if (channels.Length == 4)
        {
            // CSS alpha is 0–1; tolerate a 0–100 value so hand-written "0.5" and "50" both work.
            alpha = numbers[3] <= 1
                ? WaxColorMath.ClampPercent(numbers[3] * 100)
                : WaxColorMath.ClampPercent(numbers[3]);
        }

        return new WaxColorMath.ParsedHexColor(rgb, alpha);
    }

    /// <summary>
    /// Fills positions for stops that only appear as bare colors: known anchors stay put and the
    /// unknown ones spread evenly between their neighbors (or across 0–100 when none are positioned).
    /// </summary>
    private static List<GradientStop> DistributePositions(List<(WaxColorMath.Rgb Rgb, int Alpha, double? Position)> stops)
    {
        if (stops.All(stop => stop.Position is null))
        {
            var span = stops.Count - 1;
            return stops
                .Select((stop, index) => new GradientStop(stop.Rgb, stop.Alpha, span == 0 ? 0 : index * 100.0 / span))
                .ToList();
        }

        var result = new List<GradientStop>(stops.Count);
        for (var i = 0; i < stops.Count; i++)
        {
            if (stops[i].Position is { } known)
            {
                result.Add(new GradientStop(stops[i].Rgb, stops[i].Alpha, known));
                continue;
            }

            var lower = FindAnchor(stops, i, -1);
            var upper = FindAnchor(stops, i, +1);
            var lowPosition = lower >= 0 ? stops[lower].Position!.Value : 0;
            var highIndex = upper >= 0 ? upper : stops.Count - 1;
            var highPosition = upper >= 0 ? stops[upper].Position!.Value : 100;
            var span = highIndex - lower;
            var ratio = span == 0 ? 0 : (double)(i - lower) / span;
            result.Add(new GradientStop(stops[i].Rgb, stops[i].Alpha, lowPosition + (highPosition - lowPosition) * ratio));
        }

        return result;
    }

    private static int FindAnchor(List<(WaxColorMath.Rgb Rgb, int Alpha, double? Position)> stops, int from, int step)
    {
        for (var i = from + step; i >= 0 && i < stops.Count; i += step)
        {
            if (stops[i].Position is not null)
                return i;
        }

        return -1;
    }

    #endregion

    #region Formatting

    /// <summary>CSS value for the picker's output, e.g. <c>linear-gradient(90deg, #ff0000 0%, #0000ff 100%)</c>.</summary>
    public static string FormatGradient(WaxGradientType type, double angle, IReadOnlyList<GradientStop> stops)
    {
        var head = type switch
        {
            WaxGradientType.Linear => $"linear-gradient({FormatNumber(WaxColorMath.NormalizeHue(angle))}deg",
            WaxGradientType.Radial => "radial-gradient(circle",
            WaxGradientType.Conic => $"conic-gradient(from {FormatNumber(WaxColorMath.NormalizeHue(angle))}deg",
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        return $"{head}, {FormatStopList(stops)})";
    }

    /// <summary>Comma-separated "hex position%" list, sorted by position; used for output and the track preview.</summary>
    public static string FormatStopList(IReadOnlyList<GradientStop> stops) =>
        string.Join(", ", stops
            .OrderBy(stop => stop.Position)
            .Select(stop => $"{WaxColorMath.FormatHex(stop.Rgb, stop.AlphaPercent)} {FormatNumber(stop.Position)}%"));

    private static string FormatNumber(double value) =>
        Math.Round(value, 2, Rounding).ToString("0.##", CultureInfo.InvariantCulture);

    #endregion

    #region Helpers

    /// <summary>Two flat stops of the same color — the seed gradient when switching from solid mode.</summary>
    public static List<GradientStop> SolidStops(WaxColorMath.Rgb rgb, int alpha) =>
    [
        new(rgb, alpha, 0),
        new(rgb, alpha, 100),
    ];

    /// <summary>First stop's color — the collapse target when switching to solid mode.</summary>
    public static GradientStop FirstStop(IReadOnlyList<GradientStop> stops) =>
        stops.OrderBy(stop => stop.Position).First();

    /// <summary>Color under <paramref name="position"/> on the sorted stop list, for click-to-add on the track.</summary>
    public static (WaxColorMath.Rgb Rgb, int AlphaPercent) InterpolateColor(IReadOnlyList<GradientStop> stops, double position)
    {
        var sorted = stops.OrderBy(stop => stop.Position).ToList();

        var first = sorted[0];
        if (position <= first.Position)
            return (first.Rgb, first.AlphaPercent);

        var last = sorted[^1];
        if (position >= last.Position)
            return (last.Rgb, last.AlphaPercent);

        for (var i = 0; i < sorted.Count - 1; i++)
        {
            var left = sorted[i];
            var right = sorted[i + 1];
            if (position > right.Position)
                continue;

            var ratio = right.Position == left.Position ? 0 : (position - left.Position) / (right.Position - left.Position);
            return (
                new WaxColorMath.Rgb(
                    (int)Math.Clamp(Math.Round(left.Rgb.R + (right.Rgb.R - left.Rgb.R) * ratio, Rounding), 0, 255),
                    (int)Math.Clamp(Math.Round(left.Rgb.G + (right.Rgb.G - left.Rgb.G) * ratio, Rounding), 0, 255),
                    (int)Math.Clamp(Math.Round(left.Rgb.B + (right.Rgb.B - left.Rgb.B) * ratio, Rounding), 0, 255)),
                (int)Math.Clamp(Math.Round(left.AlphaPercent + (right.AlphaPercent - left.AlphaPercent) * ratio, Rounding), 0, 100));
        }

        return (last.Rgb, last.AlphaPercent);
    }

    #endregion
}
