#nullable enable
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public record CreateGroup(string Name);

public class GroupService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    
    public GroupService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public Group? LoadGroupByName(string name)
    {
        return _dbContext.Groups.FirstOrDefault(g => g.Name == name);
    }
    
    public ReturnValue<Group> CreateGroup(CreateGroup createGroup)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.Create_Group))
            return ErrorUtils.ValueNotFound(nameof(CreateRole), createGroup.Name);
        
        var name = createGroup.Name;
        var existingGroup = LoadGroupByName(name);
        if (existingGroup != null)
        {
            return ErrorUtils.AlreadyExists(nameof(Group), name);
        }

        var newGroup = new Group
        {
            Name = name
        };
        _dbContext.Add(newGroup);
        _dbContext.SaveChanges();
        return newGroup;
    }
}