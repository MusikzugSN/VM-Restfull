using Microsoft.AspNetCore.Http;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class UploadPdfsRequestDto
{
    public int ScoreId { get; set; }
    public List<UploadPdfFileRequestDto> Files { get; set; } = new();
}

public class UploadPdfFileRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public int VoiceId { get; set; }
    public IFormFile? File { get; set; }
}