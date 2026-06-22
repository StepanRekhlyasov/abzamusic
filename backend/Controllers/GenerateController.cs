using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/generate")]
public class GenerateController(
    SongGenerator songGenerator,
    AlbumCoverGenerator albumCoverGenerator,
    MidiGenerator midiGenerator) : ControllerBase
{
    [HttpGet("songs")]
    public ActionResult<SongsPageResponse> GetSongs(
        [FromQuery] int page,
        [FromQuery] string seed,
        [FromQuery] decimal likes,
        [FromQuery] int size,
        [FromQuery] string lang = SongLexicons.DefaultLanguage)
    {
        if (page < 1)
        {
            return BadRequest("Page must be greater than or equal to 1.");
        }

        if (!SeedParser.TryParse(seed, out var seedValue))
        {
            return BadRequest("Seed must be a valid 64-bit alphanumeric value.");
        }

        if (likes is < 0m or > 10m || likes * 10 != Math.Round(likes * 10))
        {
            return BadRequest("Likes must be between 0 and 10 with a step of 0.1.");
        }

        if (!SongLexicons.TryGet(lang, out _))
        {
            return BadRequest("Language must be 'en' or 'de'.");
        }

        return songGenerator.GeneratePage(page, seed, seedValue, likes, size, lang);
    }

    [HttpGet("cover")]
    public ActionResult GetCover(
        [FromQuery] string album,
        [FromQuery] string artist,
        [FromQuery] string title,
        [FromQuery] string seed,
        [FromQuery] string? genre = null)
    {
        if (string.IsNullOrWhiteSpace(album))
        {
            return BadRequest("Album is required.");
        }

        if (string.IsNullOrWhiteSpace(artist))
        {
            return BadRequest("Artist is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest("Title is required.");
        }

        if (!SeedParser.TryParse(seed, out var seedValue))
        {
            return BadRequest("Seed must be a valid 64-bit alphanumeric value.");
        }

        var svg = albumCoverGenerator.GenerateSvg(album, artist, title, seedValue, genre);
        return Content(svg, "image/svg+xml");
    }

    [HttpGet("midi")]
    public async Task<IActionResult> GetMidi(
        [FromQuery] string seed,
        [FromQuery] int index,
        [FromQuery] string genre,
        [FromQuery] string lang = SongLexicons.DefaultLanguage,
        CancellationToken cancellationToken = default)
    {
        if (index < 1)
        {
            return BadRequest("Index must be greater than or equal to 1.");
        }

        if (string.IsNullOrWhiteSpace(genre))
        {
            return BadRequest("Genre is required.");
        }

        if (!SeedParser.TryParse(seed, out var seedValue))
        {
            return BadRequest("Seed must be a valid 64-bit alphanumeric value.");
        }

        if (!SongLexicons.TryGet(lang, out _))
        {
            return BadRequest("Language must be 'en' or 'de'.");
        }

        var midiBytes = await midiGenerator.TryGenerateMidiAsync(seedValue, index, genre, lang, cancellationToken);
        if (midiBytes is null)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "MIDI generation is not available. Install Python with musiclang_predict.");
        }

        return File(midiBytes, "audio/midi", $"song-{index}.mid");
    }

    [HttpGet("preview")]
    public async Task<IActionResult> GetPreview(
        [FromQuery] string seed,
        [FromQuery] int index,
        [FromQuery] string genre,
        [FromQuery] string lang = SongLexicons.DefaultLanguage,
        CancellationToken cancellationToken = default)
    {
        if (index < 1)
        {
            return BadRequest("Index must be greater than or equal to 1.");
        }

        if (string.IsNullOrWhiteSpace(genre))
        {
            return BadRequest("Genre is required.");
        }

        if (!SeedParser.TryParse(seed, out var seedValue))
        {
            return BadRequest("Seed must be a valid 64-bit alphanumeric value.");
        }

        if (!SongLexicons.TryGet(lang, out _))
        {
            return BadRequest("Language must be 'en' or 'de'.");
        }

        var wavBytes = await midiGenerator.TryGeneratePreviewAsync(seedValue, index, genre, lang, cancellationToken);
        if (wavBytes is null)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Audio preview is not available. Install Python, musiclang_predict and FluidSynth.");
        }

        return File(wavBytes, "audio/wav", $"song-{index}.wav");
    }
}
