using System;
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.PrintManagementService;

namespace Vereinsmanager.Controllers.PrintManagement;

[ApiController]
[Route("api/v1/print")]
public class PrintController : ControllerBase
{
    private readonly PrintService _printService;

    public PrintController(PrintService printService)
    {
        _printService = printService;
    }

    [HttpPost]
    public ActionResult<List<string>> CreatePrintUrl([FromBody] CreatePrintRequestDto request)
    {
        if (request.MusicSheetIds == null || request.MusicSheetIds.Length == 0)
            return BadRequest("Keine IDs übergeben.");

        var result = _printService.CreatePrintUrl(request.MusicSheetIds, request.Marschbuch);

        if (result.IsSuccessful())
            return result.GetValue()!;

        return (ObjectResult)result;
    }

    [HttpPost("create-download")]
    public ActionResult<string> CreateDownloadUrl([FromBody] CreateDownloadRequestDto request)
    {
        if (request.MusicSheetIds == null || request.MusicSheetIds.Length == 0)
            return BadRequest("Keine IDs übergeben.");

        var result = _printService.CreateDownloadUrl(request.MusicSheetIds, request.AsZip, request.Marschbuch);

        if (result.IsSuccessful())
            return result.GetValue()!;

        return (ObjectResult)result;
    }

    [HttpGet("download")]
    public IActionResult DownloadByToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token fehlt.");

        var result = _printService.GetDownloadBytesByToken(token, out var contentType);

        if (!result.IsSuccessful())
            return (ObjectResult)result;

        // Wähle den Dateinamen passend zum Content-Type, damit der Browser korrekte Endung vorschlägt
        var fileName = "print.bin";
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (contentType.Contains("zip", StringComparison.OrdinalIgnoreCase))
                fileName = "print.zip";
            else if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
                fileName = "print.pdf";
        }

        return File(result.GetValue()!, contentType, fileName);
    }
}