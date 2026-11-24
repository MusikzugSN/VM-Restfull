using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vereinsmanager.Database;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/print")]
[AllowAnonymous]
public class PrintFileController : ControllerBase
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly IMemoryCache _cache;

    public PrintFileController(ServerDatabaseContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    // Route 2: welche Noten gehören zum Token (id + Nummer)
    [HttpGet("token-information")]
    public async Task<IActionResult> GetTokenInformation([FromQuery] string? token)
    {
        if (string.IsNullOrEmpty(token) || !_cache.TryGetValue("print:" + token, out PrintTokenData? data) || data == null)
            return NotFound(new { fehler = "Token ungültig" });

        var sheets = await _dbContext.MusicSheets
            .Where(m => data.MusicSheetIds.Contains(m.MusicSheetId))
            .Select(m => new { m.MusicSheetId, m.ScoreId })
            .ToListAsync();

        // Nummer aus der Notenmappe, falls eine Mappe mitgeschickt wurde
        var numbers = new Dictionary<int, string>();
        if (data.MusicFolderId != null)
        {
            numbers = await _dbContext.ScoreMusicFolders
                .Where(s => s.MusicFolderId == data.MusicFolderId)
                .ToDictionaryAsync(s => s.ScoreId, s => s.Number.ToString() ?? "");
        }

        var noten = sheets.Select(m => new
        {
            id = m.MusicSheetId,
            nummer = numbers.TryGetValue(m.ScoreId, out var n) ? n : ""
        });

        return Ok(new { token, noten });
    }

    // Route 4: eine PDF-Datei schicken
    [HttpGet("file/{id:int}")]
    public async Task<IActionResult> GetFile(int id, [FromQuery] string? token)
    {
        if (string.IsNullOrEmpty(token) || !_cache.TryGetValue("print:" + token, out PrintTokenData? data) || data == null)
            return NotFound(new { fehler = "Token ungültig" });

        if (!data.MusicSheetIds.Contains(id))
            return NotFound(new { fehler = "Notenblatt gehört nicht zum Token" });

        var sheet = await _dbContext.MusicSheets.FirstOrDefaultAsync(m => m.MusicSheetId == id);
        if (sheet == null)
            return NotFound(new { fehler = "Notenblatt nicht gefunden" });

        var path = Path.Combine("Data", "Scores", sheet.ScoreId.ToString(), sheet.FileName);
        if (!System.IO.File.Exists(path))
            return NotFound(new { fehler = "Datei nicht gefunden" });

        return PhysicalFile(Path.GetFullPath(path), "application/pdf");
    }
}
