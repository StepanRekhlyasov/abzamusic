using System.Text;

namespace backend.Services;

public class AlbumCoverGenerator(IHostEnvironment environment)
{
    private static readonly string[] CoverFonts =
    [
        "Georgia, 'Times New Roman', serif",
        "'Palatino Linotype', 'Book Antiqua', serif",
        "Impact, 'Arial Black', sans-serif",
        "'Trebuchet MS', Helvetica, sans-serif",
        "Verdana, Geneva, sans-serif",
        "Tahoma, Geneva, sans-serif",
        "'Courier New', Courier, monospace",
        "'Lucida Sans', 'Lucida Grande', sans-serif",
        "Arial, Helvetica, sans-serif",
        "'Segoe UI', Roboto, sans-serif",
        "Garamond, 'Times New Roman', serif",
        "'Franklin Gothic Medium', 'Arial Narrow', sans-serif",
    ];

    private readonly Dictionary<string, string> _backgroundDataUris = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _cacheLock = new();

    public static string BuildUrl(string album, string artist, string title, string genre, string seed)
    {
        return string.Concat(
            "/api/generate/cover?album=",
            Uri.EscapeDataString(album),
            "&artist=",
            Uri.EscapeDataString(artist),
            "&title=",
            Uri.EscapeDataString(title),
            "&genre=",
            Uri.EscapeDataString(genre),
            "&seed=",
            Uri.EscapeDataString(seed));
    }

    public string GenerateSvg(string album, string artist, string title, ulong seed, string? genre = null)
    {
        var rng = CreateRng(seed, album, artist, title);
        var fontFamily = CoverFonts[rng.Next(CoverFonts.Length)];
        var visualEffect = CoverVisualEffects.Generate(rng);
        var backgroundDataUri = GetBackgroundDataUri(genre, seed, album, artist, title);

        var albumLines = WrapLines(album, 20, 2);
        var titleLine = Truncate(title, 26);
        var artistLine = Truncate(artist, 26);

        var albumFontSize = albumLines.Count > 1 ? 11 : album.Length > 22 ? 10 : 12;
        var titleFontSize = title.Length > 22 ? 10 : 11;
        var artistFontSize = 9;

        var albumStartY = albumLines.Count > 1 ? 22 : 28;
        var titleY = 162;
        var artistY = 178;

        var svg = new StringBuilder();
        svg.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200" width="200" height="200">""");
        svg.AppendLine("""  <defs>""");
        svg.AppendLine("""    <linearGradient id="topOverlay" x1="0" y1="0" x2="0" y2="1">""");
        svg.AppendLine("""      <stop offset="0%" stop-color="rgba(0,0,0,0.78)"/>""");
        svg.AppendLine("""      <stop offset="38%" stop-color="rgba(0,0,0,0.25)"/>""");
        svg.AppendLine("""      <stop offset="100%" stop-color="rgba(0,0,0,0)"/>""");
        svg.AppendLine("""    </linearGradient>""");
        svg.AppendLine("""    <linearGradient id="bottomOverlay" x1="0" y1="0" x2="0" y2="1">""");
        svg.AppendLine("""      <stop offset="0%" stop-color="rgba(0,0,0,0)"/>""");
        svg.AppendLine("""      <stop offset="62%" stop-color="rgba(0,0,0,0.25)"/>""");
        svg.AppendLine("""      <stop offset="100%" stop-color="rgba(0,0,0,0.82)"/>""");
        svg.AppendLine("""    </linearGradient>""");
        CoverVisualEffects.AppendDefs(svg, visualEffect);
        svg.AppendLine("""  </defs>""");
        svg.AppendLine($"""  <image href="{backgroundDataUri}" x="0" y="0" width="200" height="200" preserveAspectRatio="xMidYMid slice" filter="url(#coverFx)"/>""");
        CoverVisualEffects.AppendLayers(svg, visualEffect);
        svg.AppendLine("""  <rect width="200" height="200" fill="url(#topOverlay)"/>""");
        svg.AppendLine("""  <rect width="200" height="200" fill="url(#bottomOverlay)"/>""");

        for (var i = 0; i < albumLines.Count; i++)
        {
            var y = albumStartY + i * (albumFontSize + 3);
            svg.AppendLine(BuildTextLine(albumLines[i], y, albumFontSize, fontFamily, 700, "rgba(255,255,255,0.96)", 1.2));
        }

        svg.AppendLine(BuildTextLine(titleLine, titleY, titleFontSize, fontFamily, 600, "rgba(255,255,255,0.94)", 0.6));
        svg.AppendLine(BuildTextLine(artistLine, artistY, artistFontSize, fontFamily, 500, "rgba(255,255,255,0.78)", 0.4));
        svg.AppendLine("</svg>");

        return svg.ToString();
    }

    private static Random CreateRng(ulong seed, string album, string artist, string title)
        => new(HashCode.Combine(
            unchecked((int)(seed & uint.MaxValue)),
            unchecked((int)(seed >> 32)),
            StringComparer.Ordinal.GetHashCode(album),
            StringComparer.Ordinal.GetHashCode(artist),
            StringComparer.Ordinal.GetHashCode(title)));

    private static string BuildTextLine(
        string text,
        int y,
        int fontSize,
        string fontFamily,
        int fontWeight,
        string fill,
        double letterSpacing)
        => $"""  <text x="100" y="{y}" text-anchor="middle" font-family="{EscapeXml(fontFamily)}" font-size="{fontSize}" font-weight="{fontWeight}" fill="{fill}" letter-spacing="{letterSpacing.ToString(System.Globalization.CultureInfo.InvariantCulture)}">{EscapeXml(text)}</text>""";

    private string GetBackgroundDataUri(string? genre, ulong seed, string album, string artist, string title)
    {
        var assetsDirectory = GenreCoverAssets.AssetsDirectory(environment);
        var fileName = GenreCoverAssets.PickBackgroundFileName(genre, seed, album, artist, title, assetsDirectory);
        var absolutePath = Path.Combine(assetsDirectory, fileName);

        lock (_cacheLock)
        {
            if (_backgroundDataUris.TryGetValue(fileName, out var cached))
            {
                return cached;
            }

            var bytes = File.ReadAllBytes(absolutePath);
            var dataUri = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
            _backgroundDataUris[fileName] = dataUri;
            return dataUri;
        }
    }

    private static List<string> WrapLines(string text, int maxCharsPerLine, int maxLines)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
        {
            return ["Unknown Album"];
        }

        if (normalized.Length <= maxCharsPerLine)
        {
            return [normalized];
        }

        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var lines = new List<string>();
        var current = new StringBuilder();

        foreach (var word in words)
        {
            var candidate = current.Length == 0 ? word : $"{current} {word}";
            if (candidate.Length > maxCharsPerLine && current.Length > 0)
            {
                lines.Add(current.ToString());
                current.Clear();
                current.Append(word);
            }
            else
            {
                current.Clear();
                current.Append(candidate);
            }

            if (lines.Count >= maxLines)
            {
                break;
            }
        }

        if (lines.Count < maxLines && current.Length > 0)
        {
            lines.Add(current.ToString());
        }

        if (lines.Count == 0)
        {
            lines.Add(Truncate(normalized, maxCharsPerLine));
        }

        if (lines.Count == maxLines && string.Join(' ', words).Length > string.Join(' ', lines).Length)
        {
            var last = lines[^1];
            lines[^1] = last.Length > maxCharsPerLine - 1
                ? last[..(maxCharsPerLine - 1)] + "…"
                : last + "…";
        }

        return lines;
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";

    private static string EscapeXml(string value)
        => value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}
