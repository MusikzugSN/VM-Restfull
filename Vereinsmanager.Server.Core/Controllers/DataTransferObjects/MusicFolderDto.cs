#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class MusicFolderDto : MetaDataDto
{
    public int MusicFolderId { get; init; }
    public int GroupId { get; init; }
    public string Name { get; init; }

    public MusicFolderDto(MusicFolder folder)
    {
        MusicFolderId = folder.MusicFolderId;
        GroupId = folder.GroupId;
        Name = folder.Name;

        CreatedAt = folder.CreatedAt;
        CreatedBy = folder.CreatedBy;
        UpdatedAt = folder.UpdatedAt;
        UpdatedBy = folder.UpdatedBy;
    }
}