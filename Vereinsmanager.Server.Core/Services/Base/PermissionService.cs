#nullable enable
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public class PermissionService
{
    private readonly UserContext _userContext;
    private readonly ServerDatabaseContext _dbContext;
    
    public PermissionService(UserContext userContext, ServerDatabaseContext dbContext)
    {
        _userContext = userContext;
        _dbContext = dbContext;
    }

    public bool HasPermission(PermissionType permissionType)
    {
        var user = _userContext.GetUserModel();
        if (_userContext.UserModel == null && _userContext.UserId == "-1")
            return true; // Install user has all permissions
        
        if (user == null)
            return false;
        
        if (user.IsAdmin)
            return true;
        
        return HasPermissionInternal(user, permissionType);
    }

    private bool HasPermissionInternal(User user, PermissionType permissionType, int? groupId = null, int permissionValue = 1)
    {
        if (user.IsAdmin)
            return true;

        var roles = _dbContext.UserRoles
            .Where(x => x.User.UserId == user.UserId)
            .Where(x => groupId == null || groupId == x.Group.GroupId)
            .Select(x => x.Role)
            .ToList();
        
        foreach (var role in roles)
        {
            if (HasRolePermission(role, PermissionType.Administrator, 1))
                return true;
            
            if (HasRolePermission(role, permissionType, permissionValue))
                return true;
        }

        return false;
    }
    
    private bool HasRolePermission(Role role, PermissionType permissionType, int permissionValue)
    {
        var permission = _dbContext.Permissions
            .Where(x => x.RoleId == role.RoleId)
            .FirstOrDefault(x => x.PermissionType == (int)permissionType);
        if (permission == null)
            return false;
        
        return permission.PermissionValue >= permissionValue;
    }
}