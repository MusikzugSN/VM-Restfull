namespace Vereinsmanager.Controllers.DataTransferObjects;

public class CreateMusicSheetRequestDto
{
    public required int ScoreId { get; set; }
    public required CreateMusicSheetFile[] Files { get; set; }
}

public class CreateMusicSheetFile
{
    public int? VoiceId { get; set; }
    public IFormFile? File { get; set; }
}