#nullable enable
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public record CreateGroup(string Name);
public record UpdateGroup(string Name);


public class GroupService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    
    public GroupService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public ReturnValue<Group[]> ListGroups()
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListGroup))
            return ErrorUtils.NotPermitted(nameof(Group), "read all");
        
        return _dbContext.Groups.ToArray();
    }
    
    public Group? LoadGroupByName(string name)
    {
        return _dbContext.Groups.FirstOrDefault(g => g.Name == name);
    }
    
    public Group? LoadGroupById(int id)
    {
        return _dbContext.Groups.FirstOrDefault(g => g.GroupId == id);
    }
    
    public ReturnValue<Group> CreateGroup(CreateGroup createGroup)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateGroup))
            return ErrorUtils.NotPermitted(nameof(CreateRole), createGroup.Name);
        
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

    public ReturnValue<bool> DeleteGroup(int deleteGroup)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteGroup))
            return ErrorUtils.NotPermitted(nameof(Group), deleteGroup.ToString());

        var group = LoadGroupById(deleteGroup);
        if (group == null)
            return ErrorUtils.ValueNotFound(nameof(Group), deleteGroup.ToString());
        
        _dbContext.Groups.Remove(group);
        _dbContext.SaveChanges();
        return true;
    }
    
    public ReturnValue<Group> UpdateGroup(int groupId, UpdateGroup updateGroup)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateGroup))
            return ErrorUtils.NotPermitted(nameof(Group), groupId.ToString());

        var group = _dbContext.Groups.FirstOrDefault(g => g.GroupId == groupId);
        if (group == null)
            return ErrorUtils.ValueNotFound(nameof(Group), groupId.ToString());

        // Check if name already exists (but allow same group to keep its name)
        if (!string.Equals(group.Name, updateGroup.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = LoadGroupByName(updateGroup.Name);
            if (existing != null && existing.GroupId != groupId)
                return ErrorUtils.AlreadyExists(nameof(Group), updateGroup.Name);
        }

        // Apply changes
        group.Name = updateGroup.Name;

        _dbContext.SaveChanges();
        return group;
    }
}