using backend.Models;

namespace backend.Services;

public class SongGenerator
{
    private const int InfiniteTotal = int.MaxValue;

    public SongsPageResponse GeneratePage(
        int page,
        string seedString,
        ulong seed,
        decimal likes,
        int size,
        string language)
    {
        if (size < 1)
        {
            throw new ArgumentException("Size must be greater than 0.");
        }

        SongLexicons.TryGet(language, out var lexicon);

        var startIndex = (page - 1) * size;

        var items = new List<Song>(size);
        for (var offset = 0; offset < size; offset++)
        {
            var index = startIndex + offset + 1;
            items.Add(GenerateSong(index, seedString, seed, likes, lexicon, language));
        }

        return new SongsPageResponse(page, size, InfiniteTotal, InfiniteTotal, items);
    }

    private static Song GenerateSong(
        int index,
        string seedString,
        ulong seed,
        decimal likes,
        SongLexicon lexicon,
        string language)
    {
        var rng = new Random(HashCode.Combine(
            unchecked((int)(seed & uint.MaxValue)),
            unchecked((int)(seed >> 32)),
            index,
            SongLexicons.LanguageSeed(language)));

        var title = $"{lexicon.SongAdjectives[rng.Next(lexicon.SongAdjectives.Length)]} {lexicon.SongNouns[rng.Next(lexicon.SongNouns.Length)]}";
        var artist = lexicon.Artists[rng.Next(lexicon.Artists.Length)];
        var album = $"{lexicon.AlbumPrefixes[rng.Next(lexicon.AlbumPrefixes.Length)]} {lexicon.AlbumPlaces[rng.Next(lexicon.AlbumPlaces.Length)]}";
        var genre = lexicon.Genres[rng.Next(lexicon.Genres.Length)];
        var songLikes = PickIntegerLikes(rng, likes);

        var coverUrl = AlbumCoverGenerator.BuildUrl(album, artist, title, genre, seedString);
        var previewUrl = MidiGenerator.BuildPreviewUrl(seedString, index, genre, language);
        var midiUrl = MidiGenerator.BuildMidiUrl(seedString, index, genre, language);

        return new Song(index, index, title, artist, album, genre, songLikes, coverUrl, previewUrl, midiUrl);
    }

    private static int PickIntegerLikes(Random rng, decimal likes)
    {
        var baseLikes = (int)Math.Floor(likes);
        var fractionalPart = likes - baseLikes;

        return rng.NextDouble() < (double)fractionalPart ? baseLikes + 1 : baseLikes;
    }
}
