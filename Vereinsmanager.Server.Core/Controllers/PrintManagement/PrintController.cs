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
    public ActionResult<string> CreatePrintUrl([FromBody] CreatePrintRequestDto request)
    {
        if (request.MusicSheetIds == null || request.MusicSheetIds.Length == 0)
            return BadRequest("Keine IDs übergeben.");

        var result = _printService.CreatePrintUrl(request.MusicSheetIds, request.Marschbuch);

        if (result.IsSuccessful())
            return result.GetValue()!;

        return (ObjectResult)result;
    }

    [HttpGet("download")]
    public IActionResult DownloadByToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token fehlt.");

        var result = _printService.GetPrintPdfByToken(token);

        if (!result.IsSuccessful())
            return (ObjectResult)result;

        return File(result.GetValue()!, "application/pdf", "print.pdf");
    }
}