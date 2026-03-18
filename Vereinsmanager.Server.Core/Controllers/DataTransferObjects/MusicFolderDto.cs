using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class MusicFolderDto : MetaDataDto
{
    public int MusicFolderId { get; init; }
    public int GroupId { get; init; }
    public string Name { get; init; }

    public int[]? ScoreIds { get; init; }
    public bool ShowInMyArea { get; init; }

    public MusicFolderDto(MusicFolder musicFolder)
    {
        MusicFolderId = musicFolder.MusicFolderId;
        GroupId = musicFolder.GroupId;
        Name = musicFolder.Name;
        ShowInMyArea = musicFolder.ShowInMyArea;

        ScoreIds = musicFolder.ScoreMusicFolders?
            .Select(x => x.ScoreId)
            .ToArray();
        
        ShowInMyArea  = musicFolder.ShowInMyArea;

        CreatedAt = musicFolder.CreatedAt;
        CreatedBy = musicFolder.CreatedBy;
        UpdatedAt = musicFolder.UpdatedAt;
        UpdatedBy = musicFolder.UpdatedBy;
    }
}