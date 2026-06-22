namespace backend.Models;

public record Song(
    int Id,
    int Index,
    string Title,
    string Artist,
    string Album,
    string Genre,
    int Likes,
    string CoverUrl,
    string PreviewUrl,
    string MidiUrl);
