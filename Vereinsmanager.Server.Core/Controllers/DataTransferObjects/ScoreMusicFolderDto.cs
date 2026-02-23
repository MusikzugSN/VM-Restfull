#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class ScoreMusicFolderDto : MetaDataDto
{
    public int ScoreMusicFolderId { get; init; }
    public int ScoreId { get; init; }
    public int MusicFolderId { get; init; }
    public int Number { get; init; }

    public ScoreMusicFolderDto(ScoreMusicFolder smf)
    {
        ScoreMusicFolderId = smf.ScoreMusicFolderId;
        ScoreId = smf.ScoreId;
        MusicFolderId = smf.MusicFolderId;
        Number = smf.Number;

        CreatedAt = smf.CreatedAt;
        CreatedBy = smf.CreatedBy;
        UpdatedAt = smf.UpdatedAt;
        UpdatedBy = smf.UpdatedBy;
    }
}