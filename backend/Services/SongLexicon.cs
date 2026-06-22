namespace backend.Services;

internal sealed record SongLexicon(
    string[] Genres,
    string[] Artists,
    string[] AlbumPrefixes,
    string[] AlbumPlaces,
    string[] SongAdjectives,
    string[] SongNouns);

internal static class SongLexicons
{
    public const string DefaultLanguage = "en";

    private static readonly Dictionary<string, SongLexicon> ByLanguage = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new SongLexicon(
            Genres:
            [
                "Rock", "Pop", "Jazz", "Hip-Hop", "Electronic", "Classical",
                "R&B", "Metal", "Country", "Blues", "Reggae", "Techno"
            ],
            Artists:
            [
                "The Echoes", "Luna Ray", "Midnight Bloom", "Velvet Horizon", "Neon Parade",
                "Silver Atlas", "Crimson Tide", "Aurora Fields", "Static Hearts", "Ocean Drive",
                "Paper Satellites", "Golden Frames", "Wild Syntax", "Northern Lights", "Blue Comet",
            ],
            AlbumPrefixes:
            [
                "Echoes of", "Letters from", "Stories in", "Shadows on", "Colors of",
                "Voices in", "Dreams of", "Maps to", "Fragments of", "Light in",
            ],
            AlbumPlaces:
            [
                "Tomorrow", "Midnight", "Summer", "Winter", "Neon City", "The Valley",
                "Lost Time", "Silent Rooms", "Broken Radio", "Open Roads",
            ],
            SongAdjectives:
            [
                "Silent", "Golden", "Broken", "Electric", "Fading", "Velvet", "Neon",
                "Wandering", "Crystal", "Burning", "Frozen", "Hidden",
            ],
            SongNouns:
            [
                "Rain", "Highway", "Heartbeat", "Skies", "Echo", "Horizon", "Mirrors",
                "Firelight", "Satellite", "Waves", "Parade", "Shadows",
            ]),
        ["de"] = new SongLexicon(
            Genres:
            [
                "Rock", "Pop", "Jazz", "Hip-Hop", "Elektronisch", "Klassik",
                "R&B", "Metal", "Country", "Blues", "Reggae", "Techno"
            ],
            Artists:
            [
                "Die Echos", "Luna Ray", "Mitternachtsblüte", "Samthorizont", "Neonparade",
                "Silberatlas", "Blutrote Flut", "Aurorafelder", "Statische Herzen", "Ozeanfahrt",
                "Papiersatelliten", "Goldene Rahmen", "Wilde Syntax", "Nordlichter", "Blauer Komet",
            ],
            AlbumPrefixes:
            [
                "Echos von", "Briefe aus", "Geschichten in", "Schatten auf", "Farben von",
                "Stimmen in", "Träume von", "Karten nach", "Fragmente von", "Licht in",
            ],
            AlbumPlaces:
            [
                "Morgen", "Mitternacht", "Sommer", "Winter", "Neonstadt", "Das Tal",
                "Verlorene Zeit", "Stille Räume", "Kaputtes Radio", "Offene Straßen",
            ],
            SongAdjectives:
            [
                "Still", "Golden", "Zerbrochen", "Elektrisch", "Verblassend", "Samt", "Neon",
                "Wandernd", "Kristallin", "Brennend", "Gefroren", "Verborgen",
            ],
            SongNouns:
            [
                "Regen", "Landstraße", "Herzschlag", "Himmel", "Echo", "Horizont", "Spiegel",
                "Feuerlicht", "Satellit", "Wellen", "Parade", "Schatten",
            ]),
    };

    public static bool TryGet(string language, out SongLexicon lexicon)
    {
        if (ByLanguage.TryGetValue(language, out lexicon!))
        {
            return true;
        }

        lexicon = ByLanguage[DefaultLanguage];
        return false;
    }

    public static int LanguageSeed(string language) =>
        string.Equals(language, "de", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
}
