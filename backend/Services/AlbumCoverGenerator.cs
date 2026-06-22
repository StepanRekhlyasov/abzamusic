using System.Text;

namespace backend.Services;

public class AlbumCoverGenerator(IWebHostEnvironment environment)
{
    private readonly Dictionary<string, string> _backgroundDataUris = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _cacheLock = new();

    public static string BuildUrl(string album, string artist, string genre, string seed)
    {
        return string.Concat(
            "/api/generate/cover?album=",
            Uri.EscapeDataString(album),
            "&artist=",
            Uri.EscapeDataString(artist),
            "&genre=",
            Uri.EscapeDataString(genre),
            "&seed=",
            Uri.EscapeDataString(seed));
    }

    public string GenerateSvg(string album, string artist, ulong seed, string? genre = null)
    {
        var backgroundDataUri = GetBackgroundDataUri(genre);
        var albumLines = WrapLines(album, 22, 2);
        var artistLine = Truncate(artist, 28);
        var albumFontSize = albumLines.Count > 1 ? 13 : album.Length > 24 ? 12 : 15;
        var albumStartY = albumLines.Count > 1 ? 138 : 148;

        var svg = new StringBuilder();
        svg.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200" width="200" height="200">""");
        svg.AppendLine("""  <defs>""");
        svg.AppendLine("""    <linearGradient id="overlay" x1="0" y1="0" x2="0" y2="1">""");
        svg.AppendLine("""      <stop offset="0%" stop-color="rgba(0,0,0,0.05)"/>""");
        svg.AppendLine("""      <stop offset="45%" stop-color="rgba(0,0,0,0.2)"/>""");
        svg.AppendLine("""      <stop offset="100%" stop-color="rgba(0,0,0,0.82)"/>""");
        svg.AppendLine("""    </linearGradient>""");
        svg.AppendLine("""  </defs>""");
        svg.AppendLine($"""  <image href="{backgroundDataUri}" x="0" y="0" width="200" height="200" preserveAspectRatio="xMidYMid slice"/>""");
        svg.AppendLine("""  <rect width="200" height="200" fill="url(#overlay)"/>""");

        for (var i = 0; i < albumLines.Count; i++)
        {
            var y = albumStartY + i * (albumFontSize + 4);
            svg.AppendLine($"""  <text x="100" y="{y}" text-anchor="middle" font-family="Georgia, 'Times New Roman', serif" font-size="{albumFontSize}" font-weight="700" fill="rgba(255,255,255,0.96)">{EscapeXml(albumLines[i])}</text>""");
        }

        svg.AppendLine($"""  <text x="100" y="178" text-anchor="middle" font-family="Arial, sans-serif" font-size="10" font-weight="500" fill="rgba(255,255,255,0.78)" letter-spacing="0.5">{EscapeXml(artistLine)}</text>""");
        svg.AppendLine("</svg>");

        return svg.ToString();
    }

    private string GetBackgroundDataUri(string? genre)
    {
        var cacheKey = genre ?? string.Empty;
        lock (_cacheLock)
        {
            if (_backgroundDataUris.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var relativePath = GenreCoverAssets.GetBackgroundRelativePath(genre);
            var absolutePath = ResolveAssetPath(relativePath);
            if (!File.Exists(absolutePath))
            {
                absolutePath = ResolveAssetPath(GenreCoverAssets.GetBackgroundRelativePath("Pop"));
            }

            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException($"Genre cover asset was not found: {relativePath}", absolutePath);
            }

            var bytes = File.ReadAllBytes(absolutePath);
            var dataUri = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
            _backgroundDataUris[cacheKey] = dataUri;
            return dataUri;
        }
    }

    private string ResolveAssetPath(string relativePath)
    {
        var assetsRoot = !string.IsNullOrEmpty(environment.WebRootPath)
            ? environment.WebRootPath
            : Path.Combine(environment.ContentRootPath, "wwwroot");

        return Path.Combine(assetsRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
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
