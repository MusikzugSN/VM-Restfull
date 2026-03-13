using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.PdfManagement;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;

    public PdfController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadPdfsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<UploadPdfsResponseDto> Upload([FromForm] UploadPdfsRequestDto request)
    {
        if (request.ScoreId <= 0)
        {
            return BadRequest("scoreId ist ungültig.");
        }

        if (request.Files == null || request.Files.Count == 0)
        {
            return BadRequest("Es wurden keine Dateien übergeben.");
        }

        for (int i = 0; i < request.Files.Count; i++)
        {
            UploadPdfFileRequestDto file = request.Files[i];

            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                return BadRequest($"files[{i}].fileName wurde nicht übergeben.");
            }

            if (file.VoiceId <= 0)
            {
                return BadRequest($"files[{i}].voiceId ist ungültig.");
            }

            if (file.File == null || file.File.Length == 0)
            {
                return BadRequest($"Für files[{i}] wurde keine gültige Datei übergeben.");
            }

            if (!file.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"Die Datei '{file.File.FileName}' ist keine PDF.");
            }
        }

        var result = _pdfService.UploadPdfs(request);

        if (result.IsSuccessful())
        {
            return result.GetValue();
        }

        return (ObjectResult)result;
    }

    [HttpGet("{scoreId:int}/{fileId}")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int scoreId, string fileId)
    {
        string path = _pdfService.GetPdfPath(scoreId, fileId);

        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
        {
            return NotFound();
        }

        byte[] bytes = System.IO.File.ReadAllBytes(path);
        string downloadFileName = Path.GetFileName(path);

        return File(bytes, "application/pdf", downloadFileName);
    }
}