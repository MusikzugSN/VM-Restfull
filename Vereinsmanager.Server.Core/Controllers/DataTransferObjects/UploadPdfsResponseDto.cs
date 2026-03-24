namespace Vereinsmanager.Controllers.DataTransferObjects;

public class UploadPdfsResponseDto
{
    public int ScoreId { get; set; }
    public List<UploadPdfFileResponseDto> Files { get; set; } = new();
}

public class UploadPdfFileResponseDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public int VoiceId { get; set; }
    public int MusicSheetId { get; set; }
}