using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public record TagUser(int UserId, int MusicSheetId);

public class TagDto  : MetaDataDto
{
    public int TagId { get; init; }
    public string Name { get; init; }
    public TagUser[] TagUsers { get; init; }
    
    public TagDto(Tag tag)
    {
        
        TagId = tag.TagId;
        Name = tag.Name;
        
        TagUsers = tag.TagUsers?.Select(x => new TagUser(x.UserId, x.MusicSheetId)).ToArray() ?? [];

        CreatedAt = tag.CreatedAt;
        CreatedBy = tag.CreatedBy;
        UpdatedAt = tag.UpdatedAt;
        UpdatedBy = tag.UpdatedBy;
    }
}