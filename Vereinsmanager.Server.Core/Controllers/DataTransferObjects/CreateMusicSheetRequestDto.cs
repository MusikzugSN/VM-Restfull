using Microsoft.AspNetCore.Http;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class CreateMusicSheetRequestDto
{
    public int ScoreId { get; set; }
    public List<FileMusicSheet> Files { get; set; }
}

public class FileMusicSheet
{
    public int VoiceId { get; set; }
    public bool IsMarschbuch { get; set; }
    
    public IFormFile[] Files { get; set; } = [];
}