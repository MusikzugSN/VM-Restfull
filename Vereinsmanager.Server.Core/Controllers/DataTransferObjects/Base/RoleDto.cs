#nullable enable
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Controllers.DataTransferObjects.Base;

public class RoleDto : MetaDataDto
{
    public int RoleId { get; init; }
    public string Name { get; init; }
    public List<PermissionTeaser> Permissions { get; init; }

    public RoleDto(Role role)
    {
        RoleId = role.RoleId;
        Name = role.Name;
        Permissions = role.Permissions
            .Select(p => new PermissionTeaser(p.PermissionType, p.PermissionValue))
            .ToList();
        
        UpdatedAt = role.EffectiveLastChangedAt;
        UpdatedBy = role.EffectiveLastChangedBy;
        CreatedAt = role.CreatedAt;
        CreatedBy = role.CreatedBy;
    }
}