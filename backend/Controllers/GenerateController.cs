using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/generate")]
public class GenerateController(SongGenerator songGenerator) : ControllerBase
{
    [HttpGet("songs")]
    public ActionResult<SongsPageResponse> GetSongs(
        [FromQuery] int page,
        [FromQuery] string seed,
        [FromQuery] decimal likes,
        [FromQuery] int size)
    {
        if (page < 1)
        {
            return BadRequest("Page must be greater than or equal to 1.");
        }

        if (seed is null || seed.Length != 8 || !seed.All(char.IsDigit))
        {
            return BadRequest("Seed must consist of exactly 8 digits.");
        }

        if (likes is < 0m or > 10m || likes * 10 != Math.Round(likes * 10))
        {
            return BadRequest("Likes must be between 0 and 10 with a step of 0.1.");
        }

        return songGenerator.GeneratePage(page, int.Parse(seed), likes, size);
    }
}
