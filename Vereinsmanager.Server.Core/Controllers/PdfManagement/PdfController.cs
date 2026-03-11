using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.PdfManagement;

namespace Vereinsmanager.Controllers;

[Route("[controller]")]
[ApiController]
public class PdfController : ControllerBase
{
    private readonly PdfService _pdfService;

    public PdfController(PdfService pdfService)
    {
        _pdfService = pdfService;
        Console.WriteLine("PdfController initialized");
    }

    [HttpPost("Upload")]
    [Route("[controller]/Upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        UploadPdfDto result = await _pdfService.UploadPdf(file);
        return new JsonResult(result);
    }

    [HttpPost("CreateLayout")]
    [Route("[controller]/CreateLayout")]
    public IActionResult CreateLayout([FromBody] PdfLayoutDto layout)
    {
        string path = _pdfService.CreatePdf(layout);
        byte[] bytes = System.IO.File.ReadAllBytes(path);

        return File(bytes, "application/pdf", "result.pdf");
    }

    [HttpGet("Get/{fileId}")]
    [Route("[controller]/Get/{fileId}")]
    public IActionResult Get(string fileId)
    {
        string path = _pdfService.GetPdfPath(fileId);

        if (!System.IO.File.Exists(path))
            return NotFound();

        byte[] bytes = System.IO.File.ReadAllBytes(path);
        return File(bytes, "application/pdf");
    }
}