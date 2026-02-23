#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateMusicFolder(int GroupId, string Name);
public record UpdateMusicFolder(int? GroupId, string? Name);

public record AddScoreToMusicFolder(int ScoreId, int Number);
public record UpdateScoreMusicFolder(int? Number);

public class MusicFolderService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public MusicFolderService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public MusicFolder? LoadMusicFolderById(int musicFolderId, bool includeGroup = false)
    {
        IQueryable<MusicFolder> q = _dbContext.MusicFolders;
        if (includeGroup) q = q.Include(f => f.Group);
        return q.FirstOrDefault(f => f.MusicFolderId == musicFolderId);
    }

    public ScoreMusicFolder? LoadScoreMusicFolderById(int scoreMusicFolderId, bool includeScore = false, bool includeFolder = false)
    {
        IQueryable<ScoreMusicFolder> q = _dbContext.ScoreMusicFolders;
        if (includeScore) q = q.Include(sm => sm.Score);
        if (includeFolder) q = q.Include(sm => sm.MusicFolder);
        return q.FirstOrDefault(sm => sm.ScoreMusicFolderId == scoreMusicFolderId);
    }

    public ReturnValue<MusicFolder> GetMusicFolderById(int musicFolderId, bool includeGroup = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        IQueryable<MusicFolder> q = _dbContext.MusicFolders;
        if (includeGroup) q = q.Include(f => f.Group);

        var folder = q.FirstOrDefault(f => f.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        return folder;
    }

    public ReturnValue<ScoreMusicFolder> GetScoreMusicFolderById(int scoreMusicFolderId, bool includeScore = true, bool includeFolder = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        IQueryable<ScoreMusicFolder> q = _dbContext.ScoreMusicFolders;
        if (includeScore) q = q.Include(sm => sm.Score);
        if (includeFolder) q = q.Include(sm => sm.MusicFolder);

        var link = q.FirstOrDefault(sm => sm.ScoreMusicFolderId == scoreMusicFolderId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        return link;
    }

    public ReturnValue<MusicFolder[]> ListMusicFolders(bool includeGroup = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), "read all");

        IQueryable<MusicFolder> q = _dbContext.MusicFolders;
        if (includeGroup) q = q.Include(f => f.Group);

        return q.ToArray();
    }

    public ReturnValue<ScoreMusicFolder[]> ListScoresInFolder(int musicFolderId, bool includeScore = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), "read all for folder");

        var folderExists = _dbContext.MusicFolders.Any(f => f.MusicFolderId == musicFolderId);
        if (!folderExists)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        IQueryable<ScoreMusicFolder> q = _dbContext.ScoreMusicFolders
            .Where(sm => sm.MusicFolderId == musicFolderId);

        if (includeScore) q = q.Include(sm => sm.Score);

        return q.OrderBy(sm => sm.Number).ToArray();
    }

    public ReturnValue<MusicFolder> CreateMusicFolder(CreateMusicFolder dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), dto.Name);

        var groupExists = _dbContext.Groups.Any(g => g.GroupId == dto.GroupId);
        if (!groupExists)
            return ErrorUtils.ValueNotFound(nameof(Group), dto.GroupId.ToString());

        var duplicate = _dbContext.MusicFolders.Any(f => f.GroupId == dto.GroupId && f.Name == dto.Name);
        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicFolder), $"{dto.Name} (GroupId={dto.GroupId})");

        var group = _dbContext.Groups.First(g => g.GroupId == dto.GroupId);

        var folder = new MusicFolder
        {
            GroupId = dto.GroupId,
            Group = group,
            Name = dto.Name
        };

        _dbContext.MusicFolders.Add(folder);
        _dbContext.SaveChanges();
        return folder;
    }

    public ReturnValue<ScoreMusicFolder> AddScoreToFolder(int musicFolderId, AddScoreToMusicFolder dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateScoreMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), musicFolderId.ToString());

        if (dto.Number <= 0)
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), "Number must be > 0");

        var folder = _dbContext.MusicFolders.FirstOrDefault(f => f.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == dto.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), dto.ScoreId.ToString());

        var duplicate = _dbContext.ScoreMusicFolders.Any(sm =>
            sm.MusicFolderId == musicFolderId && sm.ScoreId == dto.ScoreId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={musicFolderId}, ScoreId={dto.ScoreId}");

        var numberDuplicate = _dbContext.ScoreMusicFolders.Any(sm =>
            sm.MusicFolderId == musicFolderId && sm.Number == dto.Number);

        if (numberDuplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={musicFolderId}, Number={dto.Number}");

        var link = new ScoreMusicFolder
        {
            MusicFolderId = musicFolderId,
            MusicFolder = folder,
            ScoreId = dto.ScoreId,
            Score = score,
            Number = dto.Number
        };

        _dbContext.ScoreMusicFolders.Add(link);
        _dbContext.SaveChanges();
        return link;
    }

    public ReturnValue<MusicFolder> UpdateMusicFolder(int musicFolderId, UpdateMusicFolder dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        var folder = _dbContext.MusicFolders.FirstOrDefault(f => f.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        var newName = dto.Name ?? folder.Name;
        var newGroupId = dto.GroupId ?? folder.GroupId;

        var groupExists = _dbContext.Groups.Any(g => g.GroupId == newGroupId);
        if (!groupExists)
            return ErrorUtils.ValueNotFound(nameof(Group), newGroupId.ToString());

        var duplicate = _dbContext.MusicFolders.Any(f =>
            f.MusicFolderId != musicFolderId &&
            f.GroupId == newGroupId &&
            f.Name == newName);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicFolder), $"{newName} (GroupId={newGroupId})");

        folder.Name = newName;
        folder.GroupId = newGroupId;

        _dbContext.SaveChanges();
        return folder;
    }

    public ReturnValue<ScoreMusicFolder> UpdateScoreMusicFolder(int scoreMusicFolderId, UpdateScoreMusicFolder dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateScoreMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var link = _dbContext.ScoreMusicFolders.FirstOrDefault(sm => sm.ScoreMusicFolderId == scoreMusicFolderId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var newNumber = dto.Number ?? link.Number;

        if (newNumber <= 0)
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), "Number must be > 0");

        var numberDuplicate = _dbContext.ScoreMusicFolders.Any(sm =>
            sm.ScoreMusicFolderId != scoreMusicFolderId &&
            sm.MusicFolderId == link.MusicFolderId &&
            sm.Number == newNumber);

        if (numberDuplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={link.MusicFolderId}, Number={newNumber}");

        link.Number = newNumber;

        _dbContext.SaveChanges();
        return link;
    }

    public ReturnValue<bool> DeleteMusicFolder(int musicFolderId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        var folder = _dbContext.MusicFolders.FirstOrDefault(f => f.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        var links = _dbContext.ScoreMusicFolders.Where(sm => sm.MusicFolderId == musicFolderId).ToList();
        if (links.Count > 0)
            _dbContext.ScoreMusicFolders.RemoveRange(links);

        _dbContext.MusicFolders.Remove(folder);
        _dbContext.SaveChanges();
        return true;
    }

    public ReturnValue<bool> DeleteScoreMusicFolder(int scoreMusicFolderId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteScoreMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var link = _dbContext.ScoreMusicFolders.FirstOrDefault(sm => sm.ScoreMusicFolderId == scoreMusicFolderId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        _dbContext.ScoreMusicFolders.Remove(link);
        _dbContext.SaveChanges();
        return true;
    }
}