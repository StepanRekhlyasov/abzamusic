using System.Globalization;
using System.Text;

namespace backend.Services;

public class AlbumCoverGenerator
{
    private static readonly Dictionary<string, int> GenreHues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Rock"] = 0,
        ["Pop"] = 320,
        ["Jazz"] = 210,
        ["Hip-Hop"] = 30,
        ["Electronic"] = 280,
        ["Classical"] = 35,
        ["R&B"] = 260,
        ["Metal"] = 350,
        ["Country"] = 25,
        ["Blues"] = 220,
        ["Indie"] = 170,
        ["Reggae"] = 140,
    };

    public static string BuildUrl(string album, string seed)
    {
        return $"/api/generate/cover?album={Uri.EscapeDataString(album)}&seed={Uri.EscapeDataString(seed)}";
    }

    public string GenerateSvg(string album, ulong seed, string? genre = null)
    {
        var rng = new Random(HashCode.Combine(
            unchecked((int)(seed & uint.MaxValue)),
            unchecked((int)(seed >> 32)),
            StringComparer.Ordinal.GetHashCode(album)));

        var baseHue = genre is not null && GenreHues.TryGetValue(genre, out var genreHue)
            ? genreHue
            : Math.Abs(StringComparer.Ordinal.GetHashCode(album)) % 360;

        var hueShift = rng.Next(-25, 26);
        var hue1 = (baseHue + hueShift + 360) % 360;
        var hue2 = (hue1 + 40 + rng.Next(30, 90)) % 360;
        var sat1 = 55 + rng.Next(25);
        var sat2 = 45 + rng.Next(30);
        var lit1 = 28 + rng.Next(18);
        var lit2 = 42 + rng.Next(20);

        var colorA = Hsl(hue1, sat1, lit1);
        var colorB = Hsl(hue2, sat2, lit2);
        var accent = Hsl((hue1 + 180) % 360, 70, 65);

        var monogram = BuildMonogram(album);
        var shapes = BuildShapes(rng, accent);

        var svg = new StringBuilder();
        svg.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200" width="200" height="200">""");
        svg.AppendLine($"""  <defs><linearGradient id="bg" x1="0%" y1="0%" x2="100%" y2="100%"><stop offset="0%" stop-color="{colorA}"/><stop offset="100%" stop-color="{colorB}"/></linearGradient></defs>""");
        svg.AppendLine("""  <rect width="200" height="200" fill="url(#bg)"/>""");
        svg.Append(shapes);
        svg.AppendLine($"""  <text x="100" y="112" text-anchor="middle" font-family="Georgia, 'Times New Roman', serif" font-size="52" font-weight="700" fill="rgba(255,255,255,0.92)" letter-spacing="2">{EscapeXml(monogram)}</text>""");
        svg.AppendLine($"""  <text x="100" y="168" text-anchor="middle" font-family="Arial, sans-serif" font-size="9" fill="rgba(255,255,255,0.55)" letter-spacing="3">{EscapeXml(Truncate(album, 28).ToUpperInvariant())}</text>""");
        svg.AppendLine("</svg>");

        return svg.ToString();
    }

    private static string BuildMonogram(string album)
    {
        var words = album.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0) return "?";

        if (words.Length == 1)
        {
            return words[0].Length >= 2
                ? words[0][..2].ToUpperInvariant()
                : words[0].ToUpperInvariant();
        }

        return string.Concat(words[0][0], words[^1][0]).ToUpperInvariant();
    }

    private static string BuildShapes(Random rng, string accent)
    {
        var sb = new StringBuilder();
        var shapeCount = 4 + rng.Next(3);

        for (var i = 0; i < shapeCount; i++)
        {
            var opacity = 0.08 + rng.NextDouble() * 0.18;
            var fill = accent;

            switch (rng.Next(3))
            {
                case 0:
                {
                    var cx = rng.Next(0, 201);
                    var cy = rng.Next(0, 201);
                    var r = 20 + rng.Next(70);
                    sb.AppendLine($"""  <circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" fill-opacity="{opacity.ToString(CultureInfo.InvariantCulture)}"/>""");
                    break;
                }
                case 1:
                {
                    var x = rng.Next(-40, 160);
                    var y = rng.Next(-40, 160);
                    var w = 40 + rng.Next(100);
                    var h = 40 + rng.Next(100);
                    var rotate = rng.Next(0, 360);
                    sb.AppendLine($"""  <rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{fill}" fill-opacity="{opacity.ToString(CultureInfo.InvariantCulture)}" transform="rotate({rotate} {x + w / 2} {y + h / 2})"/>""");
                    break;
                }
                default:
                {
                    var x1 = rng.Next(0, 201);
                    var y1 = rng.Next(0, 201);
                    var x2 = rng.Next(0, 201);
                    var y2 = rng.Next(0, 201);
                    var strokeWidth = 2 + rng.Next(6);
                    sb.AppendLine($"""  <line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{fill}" stroke-opacity="{opacity.ToString(CultureInfo.InvariantCulture)}" stroke-width="{strokeWidth}"/>""");
                    break;
                }
            }
        }

        return sb.ToString();
    }

    private static string Hsl(int hue, int saturation, int lightness)
        => $"hsl({hue},{saturation}%,{lightness}%)";

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";

    private static string EscapeXml(string value)
        => value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}
