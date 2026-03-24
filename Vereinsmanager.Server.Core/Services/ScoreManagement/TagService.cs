using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateTag(string Name, string? Type = null);
public record UpdateTag(string? Name = null, string? Type = null);

public class TagService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public TagService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    private IQueryable<Tag> GetTags(bool includeTags)
    {
        IQueryable<Tag> q = _dbContext.Tags;
        if (includeTags)
            q = q.Include(i => i.Tags);
        return q;
    }

    public Tag? LoadTagById(int tagId, bool includeTags = false)
    {
        return GetTags(includeTags)
            .FirstOrDefault(i => i.TagId == tagId);
    }

    public ReturnValue<Tag[]> ListTags(bool includeTags = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListTag))
            return ErrorUtils.NotPermitted(nameof(Tag), "read all");

        return GetTags(includeTags).ToArray();
    }

    public ReturnValue<Tag> GetTagById(int tagId, bool includeTags = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListTag))
            return ErrorUtils.NotPermitted(nameof(Tag), tagId.ToString());

        var tag = GetTags(includeTags)
            .FirstOrDefault(i => i.TagId == tagId);

        if (tag == null)
            return ErrorUtils.ValueNotFound(nameof(Tag), tagId.ToString());

        return tag;
    }

    public ReturnValue<Tag> CreateTag(CreateTag dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateTag))
            return ErrorUtils.NotPermitted(nameof(Tag), dto.Name);

        var duplicate = _dbContext.Tags.Any(i => i.Name == dto.Name);
        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Tag), dto.Name);

        var tag = new Tag
        {
            Name = dto.Name,
         
        };

        _dbContext.Tags.Add(tag);
        _dbContext.SaveChanges();
        return tag;
    }

    public ReturnValue<Tag> UpdateTag(int tagId, UpdateTag dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateTag))
            return ErrorUtils.NotPermitted(nameof(Tag), tagId.ToString());

        var tag = _dbContext.Tags.FirstOrDefault(i => i.TagId == tagId);
        if (tag == null)
            return ErrorUtils.ValueNotFound(nameof(Tag), tagId.ToString());

        var newName = tag.Name;

        if (dto.Name is not null)
            newName = dto.Name;
        

        var wouldDuplicate = _dbContext.Tags.Any(i =>
            i.TagId != tagId &&
            i.Name == newName);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(Tag), newName);

        tag.Name = newName;

        _dbContext.SaveChanges();
        return tag;
    }

    public ReturnValue<bool> DeleteTag(int tagId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteTag))
            return ErrorUtils.NotPermitted(nameof(Tag), tagId.ToString());

        var tag = _dbContext.Tags
            .Include((t => t.Tags))
            .FirstOrDefault(t => t.TagId == tagId);
        
        if (tag == null)
            return ErrorUtils.ValueNotFound(nameof(Tag), tagId.ToString());
        
        if (tag.Tags != null && tag.Tags.Any())
            return ErrorUtils.NotPermitted(nameof(Tag), "delete (has Tags)");

        _dbContext.Tags.Remove(tag);
        _dbContext.SaveChanges();
        return true;
    }
} 