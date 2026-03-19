namespace Vereinsmanager.Controllers.DataTransferObjects;

public class CreatePrintRequestDto
{
    public int[] MusicSheetIds { get; set; } = [];
    public bool Marschbuch { get; set; }
}