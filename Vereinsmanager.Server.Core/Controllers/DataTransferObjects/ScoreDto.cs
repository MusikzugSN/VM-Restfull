using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class ScoreMusicFolderDto
{
    public int MusicFolderId { get; init; }
    public int Number { get; init; }

    public ScoreMusicFolderDto(ScoreMusicFolder scoreMusicFolder)
    {
        MusicFolderId = scoreMusicFolder.MusicFolderId;
        Number = scoreMusicFolder.Number;
    }
}

public class ScoreDto : MetaDataDto
{
    public int ScoreId { get; init; }
    public string Title { get; init; }
    public string Composer { get; init; }
    public string? Link { get; init; }
    public int? Duration { get; init; }

    public int[]? Sheets { get; init; }

    public ScoreMusicFolderDto[]? MusicFolders { get; init; }

    public ScoreDto(Score score)
    {
        ScoreId = score.ScoreId;
        Title = score.Title;
        Composer = score.Composer;
        Link = score.Link;
        Duration = score.Duration;

        Sheets = score.MusicSheets?
            .Select(sheet => sheet.MusicSheetId)
            .ToArray();

        MusicFolders = score.ScoreMusicFolders?
            .OrderBy(x => x.Number)
            .Select(x => new ScoreMusicFolderDto(x))
            .ToArray();

        CreatedAt = score.CreatedAt;
        CreatedBy = score.CreatedBy;
        UpdatedAt = score.UpdatedAt;
        UpdatedBy = score.UpdatedBy;
    }
}