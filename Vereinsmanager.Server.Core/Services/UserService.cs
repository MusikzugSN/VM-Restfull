#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public record UserCreate(String Username, String Password, bool? IsAdmin, List<UserRoleTeaser>? Roles);
public record UpdateUser(String? Username, String? Password, bool? IsAdmin, bool? IsEnabled, List<UserRoleTeaser>? Roles);
public record UserRoleTeaser(int RoleId, int GroupId, bool? Delete);

public class UserService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    
    public UserService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    private User? _installUserModel;
    public User GetInstallUser()
    {
        _installUserModel ??= new User
        {
            UserId = -1,
            Username = "install",
            PasswordHash = "install",
            IsAdmin = true,

            CreatedAt = DateTime.Now,
            CreatedBy = "system",
            UpdatedAt = DateTime.Now,
            UpdatedBy = "system"
        };

        return _installUserModel;
    }

    public int CountAdmins()
    {
        return _dbContext.Users.Count(x => x.IsAdmin);
    }
    
    public User? LoadUserByUsername(string username)
    {
        return _dbContext.Users.FirstOrDefault(x => x.Username == username);
    }

    public User? LoadUserById(int id)
    {
        return _dbContext.Users.FirstOrDefault(x => x.UserId == id);
    }
    
    public ReturnValue<User> CreateUser(UserCreate userCreate)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.Create_User))
            return ErrorUtils.NotPermitted(nameof(UserCreate), userCreate.Username);
        
        var username = userCreate.Username;
        var existingUser = LoadUserByUsername(username);

        if (existingUser != null)
        {
            return ErrorUtils.AlreadyExists(nameof(User), username);
        }

        var newUser = new User
        {
            Username = username,
            PasswordHash = userCreate.Password,
            IsAdmin = (userCreate.IsAdmin ?? false) && _permissionServiceLazy.Value.HasPermission(PermissionType.Administrator)
        };
        
        if (userCreate.Roles?.Count > 0)
        {
            UpdateUserRoles(newUser, userCreate.Roles);
        }
        
        _dbContext.Add(newUser);
        _dbContext.SaveChanges();
        
        return newUser;
    }

    public ReturnValue<User> UpdateUser(int id, UpdateUser updateUser)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.Update_User))
            return ErrorUtils.NotPermitted(nameof(UpdateUser), id.ToString());
        
        var userResult = LoadUserById(id);
        if (userResult is null)
        {
            return ErrorUtils.ValueNotFound(nameof(User), id.ToString());
        }

        if (updateUser.Username is not null)
        {
            userResult.Username  = updateUser.Username;
        }

        if (updateUser.Password is not null)
        {
            userResult.PasswordHash = updateUser.Password;
        }

        if (updateUser.IsAdmin is not null &&  _permissionServiceLazy.Value.HasPermission(PermissionType.Administrator))
        {
            userResult.IsAdmin = updateUser.IsAdmin ?? false;
        }

        if (updateUser.IsEnabled is not null)
        {
            userResult.IsEnabled = updateUser.IsEnabled ?? false;
        }

        if (updateUser.Roles?.Count > 0)
        {
            UpdateUserRoles(userResult, updateUser.Roles);
        }

        _dbContext.SaveChanges();
        return userResult;
    }
    
    private void UpdateUserRoles(User user, List<UserRoleTeaser> updateUserRoles)
    {
        var roleIdsToAssign = updateUserRoles.Select(x => x.RoleId).ToList();
        var rolesToAssign = _dbContext.Roles.Where(x => roleIdsToAssign.Contains(x.RoleId)).ToList();
        
        var groupIdsToAssign = updateUserRoles.Select(x => x.GroupId).ToList();
        var groupsToAssign = _dbContext.Groups.Where(x => groupIdsToAssign.Contains(x.GroupId)).ToList();
        
        var existingUserRoles = _dbContext.UserRoles
            .Include(x => x.Group).Include(x => x.Role)
            .Where(ur => ur.User.UserId == user.UserId)
            .ToList();
        
        var userRolesToRemove = existingUserRoles
            .Where(x => updateUserRoles
                .Any(y => x.Group.GroupId == y.GroupId && x.Role.RoleId == y.RoleId && (y.Delete ?? false)))
            .ToList();
        
        _dbContext.UserRoles.RemoveRange(userRolesToRemove);

        var userRolesToAdd = updateUserRoles
            .Where(x => (x.Delete ?? false) == false)
            .Select(x => new UserRole
            {
                User = user,
                Role = rolesToAssign.First(r => r.RoleId == x.RoleId),
                Group = groupsToAssign.First(g => g.GroupId == x.GroupId)
            })
            .ToList();
        
        _dbContext.UserRoles.AddRange(userRolesToAdd);
        _dbContext.SaveChanges();
    }
}