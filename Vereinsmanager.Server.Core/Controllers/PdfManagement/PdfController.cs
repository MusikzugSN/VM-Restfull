using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.PdfManagement;


namespace Vereinsmanager.Controllers.PdfManagement;

[ApiController]
[Route("api/pdf")]
public class PdfController : ControllerBase
{
    private readonly PdfService _pdfService;

    public PdfController(PdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<UploadPdfDto>> Upload(IFormFile file)
    {
        return await _pdfService.UploadPdf(file);
    }

    [HttpPost("layout")]
    public IActionResult CreateLayout([FromBody] PdfLayoutDto layout)
    {
        string path = _pdfService.CreatePdf(layout);

        byte[] bytes = System.IO.File.ReadAllBytes(path);

        return File(bytes, "application/pdf", "result.pdf");
    }

    [HttpGet("{fileId}")]
    public IActionResult Get(string fileId)
    {
        string path = _pdfService.GetPdfPath(fileId);

        if (!System.IO.File.Exists(path))
            return NotFound();

        byte[] bytes = System.IO.File.ReadAllBytes(path);

        return File(bytes, "application/pdf");
    }
}