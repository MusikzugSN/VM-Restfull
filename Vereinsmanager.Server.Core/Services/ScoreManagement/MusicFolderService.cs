using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateMusicFolder(int GroupId, string Name, bool? ShowInMyArea, List<UpdateScoreMusicFolder>? Scores);
public record UpdateMusicFolder(int? GroupId, string? Name, bool? ShowInMyArea, List<UpdateScoreMusicFolder>? Scores);

public record UpdateScoreMusicFolder(string Number, int ScoreId, bool? Deleted);

public class MusicFolderService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public MusicFolderService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public List<MusicFolder> GetMusicFoldersByName(HashSet<string?> names)
    {
        names = names.Select(name => name?.Trim()).Select(name => name?.ToLower()).ToHashSet();
        return _dbContext.MusicFolders.Where(folder => names.Contains(folder.Name.ToLower())).ToList();
    }

    public ReturnValue<MusicFolder[]> ListMusicFolders()
    {
        return ListMusicFolders(false);
    }

    public ReturnValue<MusicFolder[]> ListMusicFolders(bool includeSheets)
    {
        var folders = BuildMusicFolderQuery(includeSheets)
            .ToArray();

        var permittedFolders = folders
            .Where(folder => _permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder, folder.GroupId))
            .ToArray();

        return permittedFolders;
    }

    public ReturnValue<MusicFolder[]> ListMusicFoldersForMyArea(bool includeSheets)
    {
        var folders = BuildMusicFolderQuery(includeSheets)
            .Where(x => x.ShowInMyArea)
            .ToArray();

        var permissionFilteredFolders = folders
            .Where(x => _permissionServiceLazy.Value.HasPermission(PermissionType.OpenMyNotes, x.GroupId))
            .ToArray();

        return permissionFilteredFolders;
    }

    public ReturnValue<MusicFolder> GetMusicFolderById(int musicFolderId)
    {
        return GetMusicFolderById(musicFolderId, false);
    }

    public ReturnValue<MusicFolder> GetMusicFolderById(int musicFolderId, bool includeSheets)
    {
        var folder = BuildMusicFolderQuery(includeSheets)
            .FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);

        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder, folder.GroupId))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        return folder;
    }

    public ReturnValue<MusicFolder> CreateMusicFolder(CreateMusicFolder createMusicFolder)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicFolder, createMusicFolder.GroupId))
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
            Name = createMusicFolder.Name,
            ShowInMyArea = createMusicFolder.ShowInMyArea ?? false
        };

        _dbContext.MusicFolders.Add(folderToCreate);
        _dbContext.SaveChanges();

        if (createMusicFolder.Scores is { Count: > 0 })
        {
            var updateScoresToMusicFolderResult = UpdateScoresToMusicFolder(folderToCreate, createMusicFolder.Scores);
            if (!updateScoresToMusicFolderResult.IsSuccessful())
                return updateScoresToMusicFolderResult;

            _dbContext.SaveChanges();
        }

        return folderToCreate;
    }

    public ReturnValue<MusicFolder> UpdateMusicFolder(int musicFolderId, UpdateMusicFolder updateMusicFolder)
    {
        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicFolder, folder.GroupId))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

        var newName = updateMusicFolder.Name ?? folder.Name;
        var newGroupId = updateMusicFolder.GroupId ?? folder.GroupId;
        var showInMyArea = updateMusicFolder.ShowInMyArea ?? folder.ShowInMyArea;

        var groupExists = _dbContext.Groups.Any(groupItem => groupItem.GroupId == newGroupId);
        if (!groupExists)
            return ErrorUtils.ValueNotFound(nameof(Group), newGroupId.ToString());

        if (newGroupId != folder.GroupId &&
            !_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicFolder, newGroupId))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), $"{musicFolderId} -> GroupId={newGroupId}");

        var duplicate = _dbContext.MusicFolders.Any(folderItem =>
            folderItem.MusicFolderId != musicFolderId &&
            folderItem.GroupId == newGroupId &&
            folderItem.Name == newName);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(MusicFolder), $"{newName} (GroupId={newGroupId})");

        folder.Name = newName;
        folder.GroupId = newGroupId;
        folder.ShowInMyArea = showInMyArea;

        if (updateMusicFolder.Scores != null)
        {
            var updateScoresToMusicFolderResult = UpdateScoresToMusicFolder(folder, updateMusicFolder.Scores);
            if (!updateScoresToMusicFolderResult.IsSuccessful())
                return updateScoresToMusicFolderResult;
        }

        _dbContext.SaveChanges();
        return folder;
    }

    public ReturnValue<bool> DeleteMusicFolder(int musicFolderId)
    {
        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteMusicFolder, folder.GroupId))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), musicFolderId.ToString());

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
        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder, folder.GroupId))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), "read all for folder");

        IQueryable<ScoreMusicFolder> query = _dbContext.ScoreMusicFolders
            .Where(link => link.MusicFolderId == musicFolderId)
            .Include(link => link.Score);

        return query
            .OrderBy(link => link.Number)
            .ToArray();
    }

    public ReturnValue<ScoreMusicFolder> GetScoreMusicFolderById(int scoreMusicFolderId)
    {
        var link = _dbContext.ScoreMusicFolders
            .Include(linkItem => linkItem.Score)
            .Include(linkItem => linkItem.MusicFolder)
            .FirstOrDefault(linkItem => linkItem.ScoreMusicFolderId == scoreMusicFolderId);

        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder, link.MusicFolder.GroupId))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        return link;
    }

    public ReturnValue<ScoreMusicFolder> AddScoreToFolder(int musicFolderId, UpdateScoreMusicFolder updateScoreMusicFolder)
    {
        var folder = _dbContext.MusicFolders.FirstOrDefault(folderItem => folderItem.MusicFolderId == musicFolderId);
        if (folder == null)
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), musicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateMusicFolder, folder.GroupId))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), musicFolderId.ToString());

        var score = _dbContext.Scores.FirstOrDefault(scoreItem => scoreItem.ScoreId == updateScoreMusicFolder.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), updateScoreMusicFolder.ScoreId.ToString());

        var duplicate = _dbContext.ScoreMusicFolders.Any(link =>
            link.MusicFolderId == musicFolderId &&
            link.ScoreId == updateScoreMusicFolder.ScoreId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={musicFolderId}, ScoreId={updateScoreMusicFolder.ScoreId}");

        var numberDuplicate = _dbContext.ScoreMusicFolders.Any(link =>
            link.MusicFolderId == musicFolderId &&
            link.Number == updateScoreMusicFolder.Number);

        if (numberDuplicate)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), $"MusicFolderId={musicFolderId}, Number={updateScoreMusicFolder.Number}");

        var linkToCreate = new ScoreMusicFolder
        {
            MusicFolderId = musicFolderId,
            MusicFolder = folder,
            ScoreId = updateScoreMusicFolder.ScoreId,
            Score = score,
            Number = updateScoreMusicFolder.Number
        };

        _dbContext.ScoreMusicFolders.Add(linkToCreate);
        _dbContext.SaveChanges();
        return linkToCreate;
    }

    public ReturnValue<ScoreMusicFolder> UpdateScoreMusicFolder(int scoreMusicFolderId, UpdateScoreMusicFolder updateScoreMusicFolder)
    {
        var link = _dbContext.ScoreMusicFolders
            .Include(linkItem => linkItem.MusicFolder)
            .FirstOrDefault(linkItem => linkItem.ScoreMusicFolderId == scoreMusicFolderId);

        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateMusicFolder, link.MusicFolder.GroupId))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        var newNumber = updateScoreMusicFolder.Number;

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
        var link = _dbContext.ScoreMusicFolders
            .Include(linkItem => linkItem.MusicFolder)
            .FirstOrDefault(linkItem => linkItem.ScoreMusicFolderId == scoreMusicFolderId);

        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteMusicFolder, link.MusicFolder.GroupId))
            return ErrorUtils.NotPermitted(nameof(ScoreMusicFolder), scoreMusicFolderId.ToString());

        _dbContext.ScoreMusicFolders.Remove(link);
        _dbContext.SaveChanges();
        return true;
    }

    private ReturnValue<MusicFolder> UpdateScoresToMusicFolder(MusicFolder folder, List<UpdateScoreMusicFolder> incoming)
    {
        var normalized = incoming
            .GroupBy(x => x.ScoreId)
            .Select(g => g.Last())
            .ToList();

        var idsToDeleted = normalized
            .Where(x => x.Deleted ?? false)
            .Select(x => x.ScoreId)
            .ToHashSet();

        var entrysToDelete = _dbContext.ScoreMusicFolders
            .Where(es => es.MusicFolderId == folder.MusicFolderId)
            .Where(es => idsToDeleted.Contains(es.ScoreId))
            .ToList();

        _dbContext.RemoveRange(entrysToDelete);

        var idsToCreate = normalized
            .Where(x => (x.Deleted ?? false) == false)
            .Select(x => x.ScoreId)
            .ToHashSet();

        var existingScores = _dbContext.Scores
            .Where(x => idsToCreate.Contains(x.ScoreId))
            .ToHashSet();

        if (existingScores.Count != idsToCreate.Count)
        {
            var foundIds = existingScores.Select(x => x.ScoreId).ToHashSet();
            var missingId = idsToCreate.First(id => !foundIds.Contains(id));
            return ErrorUtils.ValueNotFound(nameof(Score), missingId.ToString());
        }

        var active = normalized
            .Where(x => (x.Deleted ?? false) == false)
            .ToList();

        var numbers = active.Select(x => x.Number).ToList();
        if (numbers.Distinct().Count() != numbers.Count)
            return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder), "duplicate Number in request");

        var numberByScoreId = active.ToDictionary(x => x.ScoreId, x => x.Number);

        var createReferences = existingScores
            .Select(CreateViaScoreId)
            .ToList();

        _dbContext.AddRange(createReferences);
        _dbContext.SaveChanges();

        return folder;

        ScoreMusicFolder CreateViaScoreId(Score score)
        {
            return new ScoreMusicFolder
            {
                MusicFolder = folder,
                Score = score,
                Number = numberByScoreId[score.ScoreId]
            };
        }
    }

    private IQueryable<MusicFolder> BuildMusicFolderQuery(bool includeSheets)
    {
        IQueryable<MusicFolder> query = _dbContext.MusicFolders;

        if (includeSheets)
            query = query.Include(folder => folder.ScoreMusicFolders);

        return query;
    }
}