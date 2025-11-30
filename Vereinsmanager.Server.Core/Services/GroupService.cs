#nullable enable
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services;

public record CreateGroup(string Name);

public class GroupService
{
    private readonly ServerDatabaseContext _dbContext;
    
    public GroupService(ServerDatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Group? LoadGroupByName(string name)
    {
        return _dbContext.Groups.FirstOrDefault(g => g.Name == name);
    }
    
    public ReturnValue<Group> CreateGroup(CreateGroup createGroup)
    {
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