using System.Collections.Frozen;
using System.Globalization;
using Microsoft.AspNetCore.Components.Forms;

namespace WaxUI.Dropzone;

/// <summary>Accept/size validation and human-readable formatting shared by the drop components.</summary>
internal static class WaxFileDrop
{
    private sealed record KnownType(string Mime, string Label, string[] Extensions);

    private static readonly KnownType[] Known =
    [
        new("image/png", "PNG", [".png"]),
        new("image/jpeg", "JPG", [".jpg", ".jpeg"]),
        new("image/webp", "WEBP", [".webp"]),
        new("image/gif", "GIF", [".gif"]),
        new("image/svg+xml", "SVG", [".svg"]),
        new("image/avif", "AVIF", [".avif"]),
        new("image/bmp", "BMP", [".bmp"]),
        new("application/pdf", "PDF", [".pdf"]),
        new("text/plain", "TXT", [".txt"]),
        new("text/csv", "CSV", [".csv"]),
        new("text/markdown", "MD", [".md", ".markdown"]),
        new("application/json", "JSON", [".json"]),
        new("application/zip", "ZIP", [".zip"]),
        new("audio/mpeg", "MP3", [".mp3"]),
        new("audio/wav", "WAV", [".wav"]),
        new("video/mp4", "MP4", [".mp4"]),
        new("video/webm", "WEBM", [".webm"]),
    ];
    
    private static readonly FrozenDictionary<string, KnownType> ByMime = BuildIndex();
    
    private static FrozenDictionary<string, KnownType> BuildIndex()
    {
        var index = Known.ToDictionary(t => t.Mime, StringComparer.OrdinalIgnoreCase);
        index["image/jpg"] = index["image/jpeg"]; // non-standard alias some clients still send
        return index.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Returns an error message, or <c>null</c> when the file is acceptable.</summary>
    /// <param name="maxSize">Maximum size in bytes; zero or negative means unlimited.</param>
    public static string? Validate(IBrowserFile file, string accept, long maxSize)
    {
        if (!IsAccepted(file, accept))
        {
            var description = DescribeAccept(accept);
            return description.Length > 0
                ? $"Unsupported file type. Accepted: {description}."
                : "Unsupported file type.";
        }

        if (maxSize > 0 && file.Size > maxSize)
            return $"That file is larger than {FormatSize(maxSize)}.";

        return null;
    }

    /// <summary>Human labels for an accept list, e.g. "image/png,image/jpeg" becomes "PNG or JPG".</summary>
    public static string DescribeAccept(string accept)
    {
        var labels = Split(accept)
            .Select(LabelFor)
            .Where(label => label.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return labels.Length switch
        {
            0 => string.Empty,
            1 => labels[0],
            _ => $"{string.Join(", ", labels[..^1])} or {labels[^1]}",
        };
    }

    /// <summary>Formats a byte count as "820 KB" / "1.5 MB" (invariant culture).</summary>
    public static string FormatSize(long bytes)
    {
        const double kb = 1024, mb = kb * 1024, gb = mb * 1024;
        var culture = CultureInfo.InvariantCulture;

        return bytes switch
        {
            >= (long)gb => string.Create(culture, $"{bytes / gb:0.#} GB"),
            >= (long)mb => string.Create(culture, $"{bytes / mb:0.#} MB"),
            >= (long)kb => string.Create(culture, $"{bytes / kb:0.#} KB"),
            _ => string.Create(culture, $"{bytes} B"),
        };
    }

    private static bool IsAccepted(IBrowserFile file, string accept)
    {
        if (string.IsNullOrWhiteSpace(accept))
            return true;

        var extension = Path.GetExtension(file.Name);
        var contentType = file.ContentType ?? string.Empty;

        foreach (var entry in Split(accept))
        {
            if (entry.StartsWith('.'))
            {
                if (string.Equals(entry, extension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (entry.EndsWith("/*", StringComparison.Ordinal))
            {
                // "image/*" -> prefix "image/"
                if (contentType.AsSpan().StartsWith(entry.AsSpan(0, entry.Length - 1), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (string.Equals(entry, contentType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (ByMime.TryGetValue(entry, out var known)
                     && known.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                // Browsers often report an empty/generic ContentType (e.g. .md, .csv); fall back to the extension.
                return true;
            }
        }

        return false;
    }

    private static string LabelFor(string entry)
    {
        if (entry.StartsWith('.'))
            return entry[1..].ToUpperInvariant();

        if (ByMime.TryGetValue(entry, out var known))
            return known.Label;

        // image/* -> images, application/* -> applications
        return entry.EndsWith("/*", StringComparison.Ordinal) ? entry[..^2] + "s" : entry;
    }

    private static IEnumerable<string> Split(string accept) =>
        accept.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
