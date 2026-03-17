#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class MusicSheetDto : MetaDataDto
{
    public int MusicSheetId { get; init; }
    public int Filesize { get; init; }
    public int PageCount { get; init; }
    public int ScoreId { get; init; }
    public int VoiceId { get; init; }
    public MusicSheetStatus Status { get; init; }

    public MusicSheetDto(MusicSheet musicSheet)
    {
        MusicSheetId = musicSheet.MusicSheetId;
        Filesize = musicSheet.Filesize;
        PageCount = musicSheet.PageCount;
        ScoreId = musicSheet.ScoreId;
        VoiceId = musicSheet.VoiceId;
        Status = musicSheet.Status;

        CreatedAt = musicSheet.CreatedAt;
        CreatedBy = musicSheet.CreatedBy;
        UpdatedAt = musicSheet.UpdatedAt;
        UpdatedBy = musicSheet.UpdatedBy;
    }
}