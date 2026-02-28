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

    public MusicSheetDto(MusicSheet sheet)
    {
        MusicSheetId = sheet.MusicSheetId;
        FilePath = sheet.FilePath;
        FileHash = sheet.FileHash;
        Filesize = sheet.Filesize;
        PageCount = sheet.PageCount;
        FileModifiedDate = sheet.FileModifiedDate;
        ScoreId = sheet.ScoreId;
        VoiceId = sheet.VoiceId;

        CreatedAt = sheet.CreatedAt;
        CreatedBy = sheet.CreatedBy;
        UpdatedAt = sheet.UpdatedAt;
        UpdatedBy = sheet.UpdatedBy;
    }
}