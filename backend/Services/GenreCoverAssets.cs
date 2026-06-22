namespace backend.Services;

public static class GenreCoverAssets
{
    private static readonly Dictionary<string, string> GenreSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Pop"] = "pop",
        ["Rock"] = "rock",
        ["Reggae"] = "reggae",
        ["Electronic"] = "electronic",
        ["Classical"] = "classical",
        ["Jazz"] = "jazz",
        ["Country"] = "country",
        ["Blues"] = "blues",
        ["Techno"] = "techno",
        ["Hip-Hop"] = "hip-hop",
        ["R&B"] = "rnb",
        ["Metal"] = "metal",
    };

    public static string AssetsDirectory(IHostEnvironment environment)
        => Path.Combine(environment.ContentRootPath, "assets", "genre-covers");

    public static string GetSlug(string? genre)
    {
        if (genre is not null && GenreSlugs.TryGetValue(genre, out var slug))
        {
            return slug;
        }

        return "pop";
    }

    public static string PickBackgroundFileName(
        string? genre,
        ulong seed,
        string album,
        string artist,
        string title,
        string assetsDirectory)
    {
        var slug = GetSlug(genre);
        var files = Directory.GetFiles(assetsDirectory, $"{slug}*.png")
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Cast<string>()
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        if (files.Count == 0)
        {
            throw new FileNotFoundException(
                $"No cover assets found for genre '{genre ?? "unknown"}' (slug: {slug}).",
                assetsDirectory);
        }

        var rng = new Random(HashCode.Combine(
            unchecked((int)(seed & uint.MaxValue)),
            unchecked((int)(seed >> 32)),
            StringComparer.Ordinal.GetHashCode(slug),
            StringComparer.Ordinal.GetHashCode(album),
            StringComparer.Ordinal.GetHashCode(artist),
            StringComparer.Ordinal.GetHashCode(title)));

        return files[rng.Next(files.Count)];
    }
}
