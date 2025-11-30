#nullable enable
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public record CreateRole(string Name);

public class RoleService
{
    private readonly ServerDatabaseContext _dbContext;
    
    public RoleService(ServerDatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Role? LoadRoleByName(string name)
    {
        return _dbContext.Roles.FirstOrDefault(role => role.Name == name);
    }

    public ReturnValue<Role> CreateRole(CreateRole createRole)
    {
        var existingRole = LoadRoleByName(createRole.Name);
        if (existingRole != null)
        {
            return ErrorUtils.AlreadyExists(nameof(Role), createRole.Name);
        }

        var newRole = new Role
        {
            Name = createRole.Name
        };
        
        _dbContext.Add(newRole);
        _dbContext.SaveChanges();
        return newRole;
    }
    
}