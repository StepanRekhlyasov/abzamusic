using backend.Models;

namespace backend.Services;

public class SongGenerator
{
    public const int TotalCount = 101;

    private static readonly string[] Genres =
    [
        "Rock", "Pop", "Jazz", "Hip-Hop", "Electronic", "Classical",
        "R&B", "Metal", "Country", "Blues", "Indie", "Reggae",
    ];

    private static readonly string[] Artists =
    [
        "The Echoes", "Luna Ray", "Midnight Bloom", "Velvet Horizon", "Neon Parade",
        "Silver Atlas", "Crimson Tide", "Aurora Fields", "Static Hearts", "Ocean Drive",
        "Paper Satellites", "Golden Frames", "Wild Syntax", "Northern Lights", "Blue Comet",
    ];

    private static readonly string[] AlbumPrefixes =
    [
        "Echoes of", "Letters from", "Stories in", "Shadows on", "Colors of",
        "Voices in", "Dreams of", "Maps to", "Fragments of", "Light in",
    ];

    private static readonly string[] AlbumPlaces =
    [
        "Tomorrow", "Midnight", "Summer", "Winter", "Neon City", "The Valley",
        "Lost Time", "Silent Rooms", "Broken Radio", "Open Roads",
    ];

    private static readonly string[] SongAdjectives =
    [
        "Silent", "Golden", "Broken", "Electric", "Fading", "Velvet", "Neon",
        "Wandering", "Crystal", "Burning", "Frozen", "Hidden",
    ];

    private static readonly string[] SongNouns =
    [
        "Rain", "Highway", "Heartbeat", "Skies", "Echo", "Horizon", "Mirrors",
        "Firelight", "Satellite", "Waves", "Parade", "Shadows",
    ];

    public SongsPageResponse GeneratePage(int page, int seed, decimal likes, int size)
    {
        if (size < 1)
        {
            throw new ArgumentException("Size must be greater than 0.");
        }

        var totalPages = (int)Math.Ceiling(TotalCount / (double)size);
        var startIndex = (page - 1) * size;
        var count = Math.Min(size, TotalCount - startIndex);

        var items = new List<Song>(count);
        for (var offset = 0; offset < count; offset++)
        {
            var index = startIndex + offset + 1;
            items.Add(GenerateSong(index, seed, likes));
        }

        return new SongsPageResponse(page, size, TotalCount, totalPages, items);
    }

    private static Song GenerateSong(int index, int seed, decimal likes)
    {
        var rng = new Random(HashCode.Combine(seed, (int)(likes * 10), index));

        var title = $"{SongAdjectives[rng.Next(SongAdjectives.Length)]} {SongNouns[rng.Next(SongNouns.Length)]}";
        var artist = Artists[rng.Next(Artists.Length)];
        var album = $"{AlbumPrefixes[rng.Next(AlbumPrefixes.Length)]} {AlbumPlaces[rng.Next(AlbumPlaces.Length)]}";
        var genre = Genres[rng.Next(Genres.Length)];
        var songLikes = RoundLikes(Math.Clamp(likes + (rng.Next(-10, 11) * 0.1m), 0m, 10m));

        return new Song(index, index, title, artist, album, genre, songLikes);
    }

    private static decimal RoundLikes(decimal value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);
}
