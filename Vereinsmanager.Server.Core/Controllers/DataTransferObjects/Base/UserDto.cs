#nullable enable
using Vereinsmanager.Database.Base;

namespace Vereinsmanager.Controllers.DataTransferObjects.Base;

public class UserDto : MetaDataDto
{
    public int UserId { get; init; }
    public string Username { get; init; }

    public bool IsAdmin { get; init; }
    
    public bool IsEnabled { get; init; }
    
    public UserGroupTeaser[] UserGroupTeasers { get; init; }
    
    public UserDto(User  user)
    {
        UserId = user.UserId;
        Username = user.Username;
        IsAdmin = user.IsAdmin;
        IsEnabled = user.IsEnabled;
        UserGroupTeasers = user.UserRoles
            .Select(ug => new UserGroupTeaser(ug.GroupId, ug.RoleId))
            .ToArray();
        
        CreatedAt = user.CreatedAt;
        CreatedBy = user.CreatedBy;
        UpdatedAt = user.EffectiveLastChangedAt;
        UpdatedBy = user.EffectiveLastChangedBy;
    }
}

public record UserGroupTeaser(int GroupId, int RoleId);