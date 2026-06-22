namespace backend.Services;

public static class GenreCoverAssets
{
    private static readonly Dictionary<string, string> GenreFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Pop"] = "pop.png",
        ["Rock"] = "rock.png",
        ["Reggae"] = "reggae.png",
        ["Electronic"] = "electronic.png",
        ["Classical"] = "classical.png",
        ["Jazz"] = "jazz.png",
        ["Country"] = "country.png",
        ["Blues"] = "blues.png",
        ["Techno"] = "techno.png",
        ["Hip-Hop"] = "hip-hop.png",
        ["R&B"] = "rnb.png",
        ["Metal"] = "metal.png",
    };

    public static string GetBackgroundPath(string? genre)
        => $"/{GetBackgroundRelativePath(genre)}";

    public static string GetBackgroundRelativePath(string? genre)
    {
        if (genre is not null && GenreFiles.TryGetValue(genre, out var file))
        {
            return $"assets/genre-covers/{file}";
        }

        return "assets/genre-covers/pop.png";
    }
}
