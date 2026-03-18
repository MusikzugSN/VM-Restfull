using Microsoft.AspNetCore.Http;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class CreateMusicSheetRequestDto
{
    public int ScoreId { get; set; }
    public int VoiceId { get; set; }
    public IFormFile? File { get; set; }
}