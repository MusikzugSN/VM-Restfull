using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class MusicFolderSheetDto
{
    public string Number { get; init; } = string.Empty;
    public int ScoreId { get; init; }
}

public class MusicFolderDto : MetaDataDto
{
    public int MusicFolderId { get; init; }
    public int GroupId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool ShowInMyArea { get; init; }
    
    public MusicFolderSheetDto[] Scores { get; init; } = Array.Empty<MusicFolderSheetDto>();


    public MusicFolderDto(MusicFolder musicFolder)
    {
        MusicFolderId = musicFolder.MusicFolderId;
        GroupId = musicFolder.GroupId;
        Name = musicFolder.Name;
        ShowInMyArea = musicFolder.ShowInMyArea;

        Scores = musicFolder.ScoreMusicFolders?
                     .Select(x => new MusicFolderSheetDto
                     {
                        
                         Number = x.Number ?? string.Empty,
                         ScoreId = x.ScoreId
                     })
                     .ToArray()
                 ?? Array.Empty<MusicFolderSheetDto>();

        CreatedAt = musicFolder.CreatedAt;
        CreatedBy = musicFolder.CreatedBy;
        UpdatedAt = musicFolder.UpdatedAt;
        UpdatedBy = musicFolder.UpdatedBy;
    }
}