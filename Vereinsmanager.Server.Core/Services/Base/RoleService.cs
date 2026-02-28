#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public record CreateRole(string Name, List<PermissionTeaser>? Permissions);

public record UpdateRole(string? Name, List<PermissionTeaser>? Permissions);
public record PermissionTeaser(int Type, int Value);

public class RoleService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    
    public RoleService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public Role? LoadRoleByName(string name)
    {
        return _dbContext.Roles.FirstOrDefault(role => role.Name == name);
    }
    
    public Role? LoadRoleById(int roleId)
    {
        return _dbContext.Roles.FirstOrDefault(role => role.RoleId == roleId);
    }

    public ReturnValue<Role[]> ListRoles()
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListRole))
            return ErrorUtils.NotPermitted(nameof(Role), "read all");
        
        return _dbContext.Roles.Include(x => x.Permissions).ToArray();
    }
    
    public ReturnValue<Role> CreateRole(CreateRole createRole)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateRole))
            return ErrorUtils.NotPermitted(nameof(CreateRole), createRole.Name);
        
        var existingRole = LoadRoleByName(createRole.Name);
        if (existingRole != null)
        {
            return ErrorUtils.AlreadyExists(nameof(Role), createRole.Name);
        }

        var newRole = new Role
        {
            Name = createRole.Name
        };

        if (createRole.Permissions?.Count > 0)
        {
            UpdatePermissions(newRole, createRole.Permissions);
        }
        
        _dbContext.Add(newRole);
        _dbContext.SaveChanges();
        return newRole;
    }
    
    public ReturnValue<bool>DeleteRole(int roleId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteRole))
            return ErrorUtils.NotPermitted(nameof(DeleteRole), roleId.ToString());
        
        var role = LoadRoleById(roleId);
        if (role == null)
        {
            return ErrorUtils.ValueNotFound(nameof(Role), roleId.ToString());
        }

        _dbContext.Roles.Remove(role);
        _dbContext.SaveChanges();
        return true;
    }
    
    public ReturnValue<Role> UpdateRole(int roleId, UpdateRole updateRole)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateRole))
            return ErrorUtils.ValueNotFound(nameof(UpdateRole), roleId.ToString());
        
        var role = LoadRoleById(roleId);
        if (role == null)
        {
            return ErrorUtils.ValueNotFound(nameof(Role), roleId.ToString());
        }

        if (updateRole.Name != null)
        {
            role.Name = updateRole.Name;
        }
        
        if (updateRole.Permissions?.Count > 0)
        {
            UpdatePermissions(role, updateRole.Permissions);
        }
        
        _dbContext.SaveChanges();
        return role;
    }

    private void UpdatePermissions(Role role, List<PermissionTeaser> updateRolePermissions)
    {
        // bestehende Berechtigungen bearbeiten
        var existingPermissions = _dbContext.Permissions.Where(p => p.Role.RoleId == role.RoleId).ToList();
        foreach (var existingPermission in existingPermissions)
        {
            existingPermission.PermissionValue = updateRolePermissions
                                                     .FirstOrDefault(x => x.Type == existingPermission.PermissionType)?.Value 
                                                 ?? existingPermission.PermissionValue;
        }
        
        // eintraege mit Standardwert loeschen
        var deleteList = existingPermissions.Where(x => x.PermissionValue == 0).ToList();
        _dbContext.Permissions.RemoveRange(deleteList);
        
        // neue Berechtigungen hinzufuegen
        var newPermissions = updateRolePermissions
            .Where(x => existingPermissions.All(y => y.PermissionType != x.Type))
            .Select(x => new Permission
            {
                Role = role,
                PermissionType = x.Type,
                PermissionValue = x.Value
            })
            .ToList();
        
        _dbContext.Permissions.AddRange(newPermissions);
    }
    
}