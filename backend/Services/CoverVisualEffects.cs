using System.Globalization;
using System.Text;

namespace backend.Services;

public readonly record struct CoverVisualEffect(
    double Brightness,
    double Contrast,
    double Sepia,
    string TintColor,
    double TintOpacity,
    string BlendMode,
    string HighlightColor,
    double HighlightOpacity,
    double HighlightCx,
    double HighlightCy,
    double HighlightRadius);

public static class CoverVisualEffects
{
    private static readonly string[] BlendModes =
    [
        "multiply",
        "overlay",
        "soft-light",
        "color",
        "hue",
    ];

    private static readonly (int R, int G, int B)[] TintPalette =
    [
        (255, 70, 90),
        (255, 150, 40),
        (80, 170, 255),
        (120, 255, 180),
        (200, 90, 255),
        (255, 220, 80),
        (40, 200, 200),
        (255, 120, 180),
    ];

    public static CoverVisualEffect Generate(Random rng)
    {
        var tint = TintPalette[rng.Next(TintPalette.Length)];
        var highlight = TintPalette[rng.Next(TintPalette.Length)];

        return new CoverVisualEffect(
            Brightness: 0.88 + rng.NextDouble() * 0.24,
            Contrast: 0.9 + rng.NextDouble() * 0.28,
            Sepia: rng.NextDouble() * 0.65,
            TintColor: $"rgb({tint.R},{tint.G},{tint.B})",
            TintOpacity: 0.14 + rng.NextDouble() * 0.26,
            BlendMode: BlendModes[rng.Next(BlendModes.Length)],
            HighlightColor: $"rgb({highlight.R},{highlight.G},{highlight.B})",
            HighlightOpacity: 0.12 + rng.NextDouble() * 0.28,
            HighlightCx: 0.12 + rng.NextDouble() * 0.76,
            HighlightCy: 0.08 + rng.NextDouble() * 0.45,
            HighlightRadius: 0.35 + rng.NextDouble() * 0.45);
    }

    public static void AppendDefs(StringBuilder svg, CoverVisualEffect effect)
    {
        var offset = 0.5 * (1 - effect.Contrast) + (effect.Brightness - 1);
        var contrast = F(effect.Contrast);
        var bias = F(offset);
        var sepia = effect.Sepia;
        var inverseSepia = 1 - sepia;

        svg.AppendLine("""    <filter id="coverFx" color-interpolation-filters="sRGB">""");
        svg.AppendLine($"""      <feColorMatrix type="matrix" values="{contrast} 0 0 0 {bias} 0 {contrast} 0 0 {bias} 0 0 {contrast} 0 {bias} 0 0 0 1 0" result="tone"/>""");
        svg.AppendLine($"""      <feColorMatrix in="tone" type="matrix" values="{BuildSepiaMatrix(inverseSepia, sepia)}"/>""");
        svg.AppendLine("""    </filter>""");

        svg.AppendLine($"""    <radialGradient id="coverHighlight" cx="{F(effect.HighlightCx)}" cy="{F(effect.HighlightCy)}" r="{F(effect.HighlightRadius)}">""");
        svg.AppendLine($"""      <stop offset="0%" stop-color="{effect.HighlightColor}" stop-opacity="{F(effect.HighlightOpacity)}"/>""");
        svg.AppendLine("""      <stop offset="100%" stop-color="rgb(255,255,255)" stop-opacity="0"/>""");
        svg.AppendLine("""    </radialGradient>""");
    }

    public static void AppendLayers(StringBuilder svg, CoverVisualEffect effect)
    {
        svg.AppendLine($"""  <rect width="200" height="200" fill="{effect.TintColor}" fill-opacity="{F(effect.TintOpacity)}" style="mix-blend-mode:{effect.BlendMode}"/>""");

        if (effect.Sepia > 0.12)
        {
            svg.AppendLine($"""  <rect width="200" height="200" fill="rgb(112,66,20)" fill-opacity="{F(effect.Sepia * 0.42)}" style="mix-blend-mode:soft-light"/>""");
        }

        svg.AppendLine("""  <rect width="200" height="200" fill="url(#coverHighlight)" style="mix-blend-mode:screen"/>""");
    }

    private static string BuildSepiaMatrix(double inverse, double sepia)
    {
        static double Blend(double inv, double sep, double identity, double sepiaValue)
            => inv * identity + sep * sepiaValue;

        return string.Join(' ',
        [
            F(Blend(inverse, sepia, 1, 0.393)), F(Blend(inverse, sepia, 0, 0.769)), F(Blend(inverse, sepia, 0, 0.189)), "0", "0",
            F(Blend(inverse, sepia, 0, 0.349)), F(Blend(inverse, sepia, 1, 0.686)), F(Blend(inverse, sepia, 0, 0.168)), "0", "0",
            F(Blend(inverse, sepia, 0, 0.272)), F(Blend(inverse, sepia, 0, 0.534)), F(Blend(inverse, sepia, 1, 0.131)), "0", "0",
            "0", "0", "0", "1", "0",
        ]);
    }

    private static string F(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}
