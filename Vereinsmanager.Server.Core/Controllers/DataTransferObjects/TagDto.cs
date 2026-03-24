using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class TagDto  : MetaDataDto
{
    public int TagId { get; init; }
    public string Name { get; init; }

    public TagDto(Tag tag)
    {
        
        TagId = tag.TagId;
        Name = tag.Name;

        CreatedAt = tag.CreatedAt;
        CreatedBy = tag.CreatedBy;
        UpdatedAt = tag.UpdatedAt;
        UpdatedBy = tag.UpdatedBy;
    }
}