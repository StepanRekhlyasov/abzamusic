namespace backend.Models;

public record SongsPageResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<Song> Items);
