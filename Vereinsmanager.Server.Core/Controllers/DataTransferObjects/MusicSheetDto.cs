using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class MusicSheetTagDto
{
    public int TagId { get; init; }
}

public class MusicSheetDto : MetaDataDto
{
    public int MusicSheetId { get; init; }
    public int Filesize { get; init; }
    public int PageCount { get; init; }
    public int ScoreId { get; init; }
    public int VoiceId { get; init; }
    public MusicSheetStatus Status { get; init; }
    public MusicSheetTagDto[] Tags { get; init; }

    public MusicSheetDto(MusicSheet musicSheet, int userId = -1)
    {
        MusicSheetId = musicSheet.MusicSheetId;
        Filesize = musicSheet.Filesize;
        PageCount = musicSheet.PageCount;
        ScoreId = musicSheet.ScoreId;
        VoiceId = musicSheet.VoiceId;
        Status = musicSheet.Status;

        Tags = musicSheet.TagUsers?.Where(x => x.UserId == userId).Select(t => new MusicSheetTagDto { TagId = t.TagId }).ToArray()
            ?? [];
    

        CreatedAt = musicSheet.CreatedAt; 
        CreatedBy = musicSheet.CreatedBy;
        UpdatedAt = musicSheet.UpdatedAt;
        UpdatedBy = musicSheet.UpdatedBy;
    }
}