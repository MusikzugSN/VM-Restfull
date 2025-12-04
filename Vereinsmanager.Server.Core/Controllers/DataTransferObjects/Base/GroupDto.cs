#nullable enable
using Vereinsmanager.Database.Base;

namespace Vereinsmanager.Controllers.DataTransferObjects.Base;

public class GroupDto : MetaDataDto
{
    public int GroupId { get; init; }
    
    public string Name { get; init; }

    public GroupDto(Group group)
    {
        GroupId = group.GroupId;
        Name = group.Name;
        
        UpdatedBy = group.UpdatedBy;
        UpdatedAt = group.UpdatedAt;
        CreatedAt = group.CreatedAt;
        CreatedBy = group.CreatedBy;
    }
}