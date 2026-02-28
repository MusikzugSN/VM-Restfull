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

    public ReturnValue<MusicFolder[]> ListMusicFolders()
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), "read all");

        return _dbContext.MusicFolders
            .OrderBy(folder => folder.Name)
            .ToArray();
    }

    public ReturnValue<MusicFolder> GetMusicFolderById(int musicFolderId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        return folder;
    }

    public ReturnValue<MusicFolder> CreateMusicFolder(CreateMusicFolder createMusicFolder)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), createMusicFolder.Name);

        var group = _dbContext.Groups.FirstOrDefault(groupItem => groupItem.GroupId == createMusicFolder.GroupId);
        if (group == null)
            return ErrorUtils.ValueNotFound(nameof(Group), createMusicFolder.GroupId.ToString());

        var duplicate = _dbContext.MusicFolders.Any(folder =>
            folder.GroupId == createMusicFolder.GroupId &&
            folder.Name == createMusicFolder.Name);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicFolder), $"{createMusicFolder.Name} (GroupId={createMusicFolder.GroupId})");

        var folderToCreate = new MusicFolder
        {
            GroupId = createMusicFolder.GroupId,
            Group = group,
            Name = createMusicFolder.Name
        };

        _dbContext.MusicFolders.Add(folderToCreate);
        _dbContext.SaveChanges();
        return folderToCreate;
    }

    public ReturnValue<MusicFolder> UpdateMusicFolder(int musicFolderId, UpdateMusicFolder updateMusicFolder)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        var newName = updateMusicFolder.Name ?? folder.Name;
        var newGroupId = updateMusicFolder.GroupId ?? folder.GroupId;

        var groupExists = _dbContext.Groups.Any(groupItem => groupItem.GroupId == newGroupId);
        if (!groupExists)
            return ErrorUtils.ValueNotFound(nameof(Group), newGroupId.ToString());

        var duplicate = _dbContext.MusicFolders.Any(folderItem =>
            folderItem.MusicFolderId != musicFolderId &&
            folderItem.GroupId == newGroupId &&
            folderItem.Name == newName);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicFolder), $"{newName} (GroupId={newGroupId})");

        folder.Name = newName;
        folder.GroupId = newGroupId;

        _dbContext.SaveChanges();
        return folder;
    }

    public ReturnValue<bool> DeleteMusicFolder(int musicFolderId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        var links = _dbContext.ScoreMusicFolders
            .Where(link => link.MusicFolderId == musicFolderId)
            .ToList();

        if (links.Count > 0)
            _dbContext.ScoreMusicFolders.RemoveRange(links);

        _dbContext.MusicFolders.Remove(folder);
        _dbContext.SaveChanges();
        return true;
    }

    public ReturnValue<ScoreMusicFolder[]> ListScoresInFolder(int musicFolderId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), "read all for folder");

        var folderExists = _dbContext.MusicFolders.Any(folder => folder.MusicFolderId == musicFolderId);
        if (!folderExists)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        IQueryable<ScoreMusicFolder> query = _dbContext.ScoreMusicFolders
            .Where(link => link.MusicFolderId == musicFolderId)
            .Include(link => link.Score);

        return query
            .OrderBy(link => link.Number)
            .ToArray();
    }

    public ReturnValue<ScoreMusicFolder> GetScoreMusicFolderById(int scoreMusicFolderId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var link = _dbContext.ScoreMusicFolders
            .Include(linkItem => linkItem.Score)
            .Include(linkItem => linkItem.MusicFolder)
            .FirstOrDefault(linkItem => linkItem.ScoreMusicFolderId == scoreMusicFolderId);

        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        return link;
    }

    public ReturnValue<ScoreMusicFolder> AddScoreToFolder(int musicFolderId, AddScoreToMusicFolder addScoreToMusicFolder)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateScoreMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), musicFolderId.ToString());

        if (addScoreToMusicFolder.Number <= 0)
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), "Number must be > 0");

        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        var score = _dbContext.Scores.FirstOrDefault(scoreItem => scoreItem.ScoreId == addScoreToMusicFolder.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), addScoreToMusicFolder.ScoreId.ToString());

        var duplicate = _dbContext.ScoreMusicFolders.Any(link =>
            link.MusicFolderId == musicFolderId &&
            link.ScoreId == addScoreToMusicFolder.ScoreId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={musicFolderId}, ScoreId={addScoreToMusicFolder.ScoreId}");

        var numberDuplicate = _dbContext.ScoreMusicFolders.Any(link =>
            link.MusicFolderId == musicFolderId &&
            link.Number == addScoreToMusicFolder.Number);

        if (numberDuplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={musicFolderId}, Number={addScoreToMusicFolder.Number}");

        var linkToCreate = new ScoreMusicFolder
        {
            MusicFolderId = musicFolderId,
            MusicFolder = folder,
            ScoreId = addScoreToMusicFolder.ScoreId,
            Score = score,
            Number = addScoreToMusicFolder.Number
        };

        _dbContext.ScoreMusicFolders.Add(linkToCreate);
        _dbContext.SaveChanges();
        return linkToCreate;
    }

    public ReturnValue<ScoreMusicFolder> UpdateScoreMusicFolder(int scoreMusicFolderId, UpdateScoreMusicFolder updateScoreMusicFolder)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateScoreMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var link = _dbContext.ScoreMusicFolders.FirstOrDefault(linkItem => linkItem.ScoreMusicFolderId == scoreMusicFolderId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var newNumber = updateScoreMusicFolder.Number ?? link.Number;

        if (newNumber <= 0)
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), "Number must be > 0");

        var numberDuplicate = _dbContext.ScoreMusicFolders.Any(linkItem =>
            linkItem.ScoreMusicFolderId != scoreMusicFolderId &&
            linkItem.MusicFolderId == link.MusicFolderId &&
            linkItem.Number == newNumber);

        if (numberDuplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={link.MusicFolderId}, Number={newNumber}");

        link.Number = newNumber;

        _dbContext.SaveChanges();
        return link;
    }

    public ReturnValue<bool> DeleteScoreMusicFolder(int scoreMusicFolderId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteScoreMusicFolder))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var link = _dbContext.ScoreMusicFolders.FirstOrDefault(linkItem => linkItem.ScoreMusicFolderId == scoreMusicFolderId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        _dbContext.ScoreMusicFolders.Remove(link);
        _dbContext.SaveChanges();
        return true;
    }
}