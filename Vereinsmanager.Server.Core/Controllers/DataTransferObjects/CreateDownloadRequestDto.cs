namespace Vereinsmanager.Controllers.DataTransferObjects;

public class CreateDownloadRequestDto
{
    public int[] MusicSheetIds { get; set; } = [];
    public bool AsZip { get; set; }
    public bool Marschbuch { get; set; }
}

