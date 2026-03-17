using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateScore(string Title, string Composer, string? Link, double? Duration, List<UpdateMusicFolderScore> MusicFolders);
public record CreateMultipleScore(string Title, string Composer, string? Link, double? Duration, string? FolderName, string? Number);
public record UpdateScore(string? Title, string? Composer, string? Link, double? Duration, List<UpdateMusicFolderScore> MusicFolders);
public record UpdateMusicFolderScore(int MusicFolderId, string Number, bool? Deleted);
public class ScoreService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;
    
    private readonly Lazy<MusicFolderService> _folderServiceLazy;
    private readonly Lazy<GroupService> _groupServiceLazy;

    public ScoreService(ServerDatabaseContext dbContext, 
        Lazy<PermissionService> permissionServiceLazy, 
        Lazy<MusicFolderService> folderServiceLazy,
        Lazy<GroupService> groupService)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
        
        _folderServiceLazy = folderServiceLazy;
        _groupServiceLazy = groupService;
    }

    private Score? LoadScoreById(int scoreId)
    {
        return _dbContext.Scores.FirstOrDefault(score => score.ScoreId == scoreId);
    }

    private static bool IsValidHttpsLink(string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return false;

        return Uri.TryCreate(link, UriKind.Absolute, out var uri) &&
               string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }

    public ReturnValue<Score[]> ListScores(bool includeMusicSheets = false, bool includeMusicFolders = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListScore))
            return ErrorUtils.NotPermitted(nameof(Score), "read all");
        
        IQueryable<Score> scoresQuery = _dbContext.Scores;
        
        if (includeMusicSheets)
            scoresQuery = scoresQuery.Include(s => s.MusicSheets);

        if (includeMusicFolders)
            scoresQuery = scoresQuery
                .Include(s => s.ScoreMusicFolders);

        return scoresQuery.ToArray();
    }

    public ReturnValue<Score> GetScoreById(int scoreId, bool includeSheets = false, bool includeMusicFolders = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListScore))
            return ErrorUtils.NotPermitted(nameof(Score), scoreId.ToString());
        
        IQueryable<Score> query = _dbContext.Scores;

        if (includeSheets)
            query = query.Include(s => s.MusicSheets);

        if (includeMusicFolders)
            query = query.Include(s => s.ScoreMusicFolders);

        var score = query.FirstOrDefault(s => s.ScoreId == scoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), scoreId.ToString());

        return score;
    }

    public ReturnValue<Score> CreateScore(CreateScore createScore)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateScore))
            return ErrorUtils.NotPermitted(nameof(Score), createScore.Title);

        if (createScore.Duration is not null && createScore.Duration <= 0)
            return ErrorUtils.ValueOutOfRange(nameof(Score), identifier: "Duration must be > 0");

        if (createScore.Link != null && !IsValidHttpsLink(createScore.Link))
            return ErrorUtils.ValueValidationFailed(nameof(Score), identifier: "Link must start with https://");

        var duplicate = _dbContext.Scores.Any(score => score.Title == createScore.Title);
        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Score), createScore.Title);

        var newScore = new Score
        {
            Title = createScore.Title,
            Composer = createScore.Composer,
            Link = createScore.Link,
            Duration = createScore.Duration
        };

        _dbContext.Scores.Add(newScore);
        _dbContext.SaveChanges();

        if (createScore.MusicFolders is { Count: > 0 })
        {
            var result = UpdateMusicFoldersToScore(newScore, createScore.MusicFolders);
            if (!result.IsSuccessful())
                return result;

            _dbContext.SaveChanges();
        }

        return newScore;
    }
    
    public ReturnValue<Score[]> CreateMultipleScores(List<CreateMultipleScore> createScores)
    {
        var folderNames = createScores.Select(c => c.FolderName)
            .Where(fn => !string.IsNullOrWhiteSpace(fn))
            .ToHashSet();

        var folders = _folderServiceLazy.Value.GetMusicFoldersByName(folderNames);
        
        //todo far: berechtigung für die Gruppen prüfen - nicht generel
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateScore))
        {
            return ErrorUtils.NotPermitted(nameof(Score), "read all");
        }
        
        // Gruppen laden (für neue Folder)
        var groupsResult = _groupServiceLazy.Value.ListGroups();
        if (!groupsResult.IsSuccessful())
            return ErrorUtils.ValueNotFound(nameof(Group), "no groups available");

        var defaultGroup = groupsResult.GetValue()!.First();
        
        var scoresToSave = new List<Score>(createScores.Count);
        var scoreMusicFolderToSave = new List<ScoreMusicFolder>(createScores.Count(x => x.Number != null));
        var foldersToCreate = new List<MusicFolder>();
        
        foreach (var createMultipleScore in createScores)
        {
            if (string.IsNullOrWhiteSpace(createMultipleScore.Title))
                continue; // skip invalid entries
            
            if (scoresToSave.Any(x => x.Title == createMultipleScore.Title))
            {
                continue; // skip duplicates
            }
            
            var score = new Score
            {
                Title = createMultipleScore.Title,
                Composer = createMultipleScore.Composer,
                Link = createMultipleScore.Link,
                Duration = createMultipleScore.Duration
            };
            
            scoresToSave.Add(score);
            
            if (createMultipleScore.FolderName != null && createMultipleScore.Number != null)
            {
                var folder = folders.FirstOrDefault(f => f.Name == createMultipleScore.FolderName);
                if (folder == null)
                {
                    folder = new MusicFolder
                    {
                        GroupId = defaultGroup.GroupId,
                        Group = defaultGroup,
                        Name = createMultipleScore.FolderName
                    };

                    foldersToCreate.Add(folder);
                    folders.Add(folder);
                }

                var scoreMusicFolder = new ScoreMusicFolder
                {
                    Score = score,
                    MusicFolder = folder,
                    Number = createMultipleScore.Number
                };
                
                scoreMusicFolderToSave.Add(scoreMusicFolder);
            }
        }
        
        _dbContext.MusicFolders.AddRange(foldersToCreate);
        _dbContext.Scores.AddRange(scoresToSave);
        _dbContext.ScoreMusicFolders.AddRange(scoreMusicFolderToSave);
        _dbContext.SaveChanges();
        return scoresToSave.ToArray();
    }

    public ReturnValue<Score> UpdateScore(int scoreId, UpdateScore updateScore)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateScore))
            return ErrorUtils.NotPermitted(nameof(Score), scoreId.ToString());

        var score = LoadScoreById(scoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), scoreId.ToString());

        if (updateScore.Title is not null)
            score.Title = updateScore.Title;

        if (updateScore.Composer is not null)
            score.Composer = updateScore.Composer;

        if (updateScore.Link is not null)
        {
            if (!IsValidHttpsLink(updateScore.Link))
                return ErrorUtils.ValueValidationFailed(nameof(Score), "Link must be https://");

            score.Link = updateScore.Link;
        }

        if (updateScore.Duration is not null)
        {
            if (updateScore.Duration.Value <= 0)
                return ErrorUtils.ValueOutOfRange(nameof(Score), "Duration must be > 0");

            score.Duration = updateScore.Duration.Value;
        }

        var wouldDuplicate = _dbContext.Scores.Any(existing =>
            existing.ScoreId != scoreId &&
            existing.Title == score.Title);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(Score), score.Title);

        if (updateScore.MusicFolders != null)
        {
            var result = UpdateMusicFoldersToScore(score, updateScore.MusicFolders);
            if (!result.IsSuccessful())
                return result;
        }

        _dbContext.SaveChanges();
        return score;
    }

    public ReturnValue<bool> DeleteScore(int scoreId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteScore))
            return ErrorUtils.NotPermitted(nameof(Score), scoreId.ToString());

        var score = LoadScoreById(scoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), scoreId.ToString());

        var hasSheets = _dbContext.MusicSheets.Any(sheet => sheet.ScoreId == scoreId);
        if (hasSheets)
            return ErrorUtils.NotPermitted(nameof(Score), "delete (has MusicSheets)");

        var folderLinks = _dbContext.ScoreMusicFolders
            .Where(link => link.ScoreId == scoreId)
            .ToList();

        if (folderLinks.Count > 0)
            _dbContext.ScoreMusicFolders.RemoveRange(folderLinks);

        _dbContext.Scores.Remove(score);
        _dbContext.SaveChanges();
        return true;
    }

    private ReturnValue<Score> UpdateMusicFoldersToScore(Score score, List<UpdateMusicFolderScore> incoming)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListMusicFolder))
            return ErrorUtils.NotPermitted(nameof(MusicFolder), "read all");

        var normalized = incoming
            .GroupBy(x => x.MusicFolderId)
            .Select(g => g.Last())
            .ToList();

        var active = normalized
            .Where(x => (x.Deleted ?? false) == false)
            .ToList();

        var folderIds = active.Select(x => x.MusicFolderId).ToHashSet();

        var folders = _dbContext.MusicFolders
            .Where(x => folderIds.Contains(x.MusicFolderId))
            .ToList();

        if (folders.Count != folderIds.Count)
        {
            var foundIds = folders.Select(x => x.MusicFolderId).ToHashSet();
            var missingId = folderIds.First(id => !foundIds.Contains(id));
            return ErrorUtils.ValueNotFound(nameof(MusicFolder), missingId.ToString());
        }

        var existingLinks = _dbContext.ScoreMusicFolders
            .Where(x => x.ScoreId == score.ScoreId)
            .ToList();

        var folderIdsToDelete = normalized
            .Where(x => x.Deleted ?? false)
            .Select(x => x.MusicFolderId)
            .ToHashSet();

        var linksToDelete = existingLinks
            .Where(x => folderIdsToDelete.Contains(x.MusicFolderId))
            .ToList();

        if (linksToDelete.Count > 0)
            _dbContext.ScoreMusicFolders.RemoveRange(linksToDelete);

        foreach (var item in active)
        {
            var numberAlreadyUsed = _dbContext.ScoreMusicFolders.Any(x =>
                x.MusicFolderId == item.MusicFolderId &&
                x.ScoreId != score.ScoreId &&
                x.Number == item.Number);

            if (numberAlreadyUsed)
                return ErrorUtils.AlreadyExists(nameof(ScoreMusicFolder),
                    $"MusicFolderId={item.MusicFolderId}, Number={item.Number}");

            var existingLink = existingLinks.FirstOrDefault(x => x.MusicFolderId == item.MusicFolderId);

            if (existingLink != null)
            {
                existingLink.Number = item.Number;
            }
            else
            {
                var existingFolder = folders.First(x => x.MusicFolderId == item.MusicFolderId);

                _dbContext.ScoreMusicFolders.Add(new ScoreMusicFolder
                {
                    ScoreId = score.ScoreId,
                    Score = score,
                    MusicFolderId = item.MusicFolderId,
                    MusicFolder = existingFolder,
                    Number = item.Number
                });
            }
        }

        return score;
    }
}