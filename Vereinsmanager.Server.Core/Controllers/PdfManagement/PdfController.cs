using Microsoft.AspNetCore.Mvc;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PdfController : ControllerBase
{
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload()
    {
        Console.WriteLine("=== Upload Request ===");

        string? scoreId = Request.Form["scoreId"];
        Console.WriteLine($"ScoreId: {scoreId}");

        List<object> files = new();

        for (int i = 0; i < Request.Form.Files.Count; i++)
        {
            var file = Request.Form.Files[i];

            string fileName = Request.Form[$"files[{i}].fileName"];
            string voiceId = Request.Form[$"files[{i}].voiceId"];

            Console.WriteLine($"File {i}");
            Console.WriteLine($"fileName: {fileName}");
            Console.WriteLine($"voiceId: {voiceId}");
            Console.WriteLine($"uploaded file: {file.FileName}");
            Console.WriteLine($"size: {file.Length}");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            files.Add(new
            {
                index = i,
                fileName = fileName,
                voiceId = voiceId,
                uploadedFileName = file.FileName,
                sizeBytes = file.Length
            });
        }

        return Ok(new
        {
            message = "Upload received",
            scoreId = scoreId,
            fileCount = Request.Form.Files.Count,
            files = files
        });
    }
}