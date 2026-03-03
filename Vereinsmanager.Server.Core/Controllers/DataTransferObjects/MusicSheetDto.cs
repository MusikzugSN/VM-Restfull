#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class MusicSheetDto : MetaDataDto
{
    public int MusicSheetId { get; init; }
    public string FilePath { get; init; }
    public string FileHash { get; init; }
    public int Filesize { get; init; }
    public int PageCount { get; init; }
    public DateTime FileModifiedDate { get; init; }
    public int ScoreId { get; init; }
    public int VoiceId { get; init; }

    public MusicSheetDto(MusicSheet musicSheet)
    {
        MusicSheetId = musicSheet.MusicSheetId;
        FileHash = musicSheet.FileHash;
        Filesize = musicSheet.Filesize;
        PageCount = musicSheet.PageCount;
        ScoreId = musicSheet.ScoreId;
        VoiceId = musicSheet.VoiceId;

        CreatedAt = musicSheet.CreatedAt;
        CreatedBy = musicSheet.CreatedBy;
        UpdatedAt = musicSheet.UpdatedAt;
        UpdatedBy = musicSheet.UpdatedBy;
    }
}