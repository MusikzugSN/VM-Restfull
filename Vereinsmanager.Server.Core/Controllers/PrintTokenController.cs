using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Vereinsmanager.Database;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/print")]
[AllowAnonymous]
public class PrintTokenController : ControllerBase
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly IMemoryCache _cache;

    public PrintTokenController(ServerDatabaseContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    // Route 1: Token erstellen
    [HttpPost("token")]
    public async Task<IActionResult> CreateToken([FromBody] CreatePrintTokenRequest request)
    {
        if (request.MusicSheetIds == null || request.MusicSheetIds.Length == 0)
            return BadRequest(new { fehler = "Keine Noten ausgewählt" });

        var ids = request.MusicSheetIds.Distinct().ToArray();
        var found = await _dbContext.MusicSheets.CountAsync(m => ids.Contains(m.MusicSheetId));
        if (found != ids.Length)
            return NotFound(new { fehler = "Notenblatt nicht gefunden" });

        var token = Guid.NewGuid().ToString("N");
        _cache.Set("print:" + token, new PrintTokenData(ids, request.MusicFolderId), TimeSpan.FromMinutes(30));

        return Ok(new { token });
    }

    // Route 3: Druckeinstellungen
    [HttpGet("print-information")]
    public async Task<IActionResult> GetPrintInformation([FromQuery] string? token)
    {
        if (string.IsNullOrEmpty(token) || !_cache.TryGetValue("print:" + token, out PrintTokenData? _))
            return NotFound(new { fehler = "Token ungültig" });

        var settings = await _dbContext.PrintSettings
            .Select(p => new { p.PrintConfigId, p.PageCount, p.Mode, p.Duplex, p.FileFormat })
            .ToListAsync();

        return Ok(settings);
    }
}