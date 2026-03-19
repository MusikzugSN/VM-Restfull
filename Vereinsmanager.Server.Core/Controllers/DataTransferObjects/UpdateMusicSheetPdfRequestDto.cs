using Microsoft.AspNetCore.Http;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class UpdateMusicSheetPdfRequestDto
{
    public IFormFile? File { get; set; }
    public IFormFile? SecondFile { get; set; }
}