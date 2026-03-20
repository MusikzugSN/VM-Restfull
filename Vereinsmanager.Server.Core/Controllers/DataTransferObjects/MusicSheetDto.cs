#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class MusicSheetFileDto
{
    public int MusicSheetFileId { get; init; }
    public int SortOrder { get; init; }
    public int Filesize { get; init; }
    public int PageCount { get; init; }

    public MusicSheetFileDto(MusicSheetFile file)
    {
        MusicSheetFileId = file.MusicSheetFileId;
        SortOrder = file.SortOrder;
        Filesize = file.Filesize;
        PageCount = file.PageCount;
    }
}

public class MusicSheetDto : MetaDataDto
{
    public int MusicSheetId { get; init; }
    public int Filesize { get; init; }
    public int PageCount { get; init; }
    public int ScoreId { get; init; }
    public int VoiceId { get; init; }
    public bool IsMarschbuch { get; init; }
    public MusicSheetStatus Status { get; init; }
    public int FileCount { get; init; }
    public MusicSheetFileDto[] Files { get; init; }

    public MusicSheetDto(MusicSheet musicSheet)
    {
        MusicSheetId = musicSheet.MusicSheetId;
        Filesize = musicSheet.Filesize;
        PageCount = musicSheet.PageCount;
        ScoreId = musicSheet.ScoreId;
        VoiceId = musicSheet.VoiceId;
        IsMarschbuch = musicSheet.IsMarschbuch;
        Status = musicSheet.Status;

        var orderedFiles = musicSheet.Files?
            .OrderBy(x => x.SortOrder)
            .ToArray() ?? [];

        FileCount = orderedFiles.Length;
        Files = orderedFiles
            .Select(x => new MusicSheetFileDto(x))
            .ToArray();

        CreatedAt = musicSheet.CreatedAt;
        CreatedBy = musicSheet.CreatedBy;
        UpdatedAt = musicSheet.UpdatedAt;
        UpdatedBy = musicSheet.UpdatedBy;
    }
}