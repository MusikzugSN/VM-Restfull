#nullable enable
using Vereinsmanager.Database.Base;

namespace Vereinsmanager.DataTransferObjects.Base;

public class RoleDto : MetaDataDto
{
    public int RoleId { get; init; }
    
    public string Name { get; init; }

    public RoleDto(Role role)
    {
        RoleId = role.RoleId;
        Name = role.Name;
        
        UpdatedAt = role.UpdatedAt;
        UpdatedBy = role.UpdatedBy;
        CreatedAt = role.CreatedAt;
        CreatedBy = role.CreatedBy;
    }
}