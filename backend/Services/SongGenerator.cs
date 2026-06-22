using backend.Models;

namespace backend.Services;

public class SongGenerator
{
    private const int InfiniteTotal = int.MaxValue;

    private static readonly string[] Genres =
    [
        "Rock", "Pop", "Jazz", "Hip-Hop", "Electronic", "Classical",
        "R&B", "Metal", "Country", "Blues", "Reggae", "Techno"
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

    public SongsPageResponse GeneratePage(int page, string seedString, ulong seed, decimal likes, int size)
    {
        if (size < 1)
        {
            throw new ArgumentException("Size must be greater than 0.");
        }

        var startIndex = (page - 1) * size;

        var items = new List<Song>(size);
        for (var offset = 0; offset < size; offset++)
        {
            var index = startIndex + offset + 1;
            items.Add(GenerateSong(index, seedString, seed, likes));
        }

        return new SongsPageResponse(page, size, InfiniteTotal, InfiniteTotal, items);
    }

    private static Song GenerateSong(int index, string seedString, ulong seed, decimal likes)
    {
        var rng = new Random(HashCode.Combine(
            unchecked((int)(seed & uint.MaxValue)),
            unchecked((int)(seed >> 32)),
            (int)(likes * 10),
            index));

        var title = $"{SongAdjectives[rng.Next(SongAdjectives.Length)]} {SongNouns[rng.Next(SongNouns.Length)]}";
        var artist = Artists[rng.Next(Artists.Length)];
        var album = $"{AlbumPrefixes[rng.Next(AlbumPrefixes.Length)]} {AlbumPlaces[rng.Next(AlbumPlaces.Length)]}";
        var genre = Genres[rng.Next(Genres.Length)];
        var songLikes = PickIntegerLikes(rng, likes);

        var coverUrl = AlbumCoverGenerator.BuildUrl(album, artist, genre, seedString);

        return new Song(index, index, title, artist, album, genre, songLikes, coverUrl);
    }

    private static int PickIntegerLikes(Random rng, decimal likes)
    {
        var baseLikes = (int)Math.Floor(likes);
        var fractionalPart = likes - baseLikes;

        return rng.NextDouble() < (double)fractionalPart ? baseLikes + 1 : baseLikes;
    }
}
