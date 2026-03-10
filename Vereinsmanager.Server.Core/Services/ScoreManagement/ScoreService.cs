using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateScore(string Title, string Composer, string? Link, int? Duration);
public record UpdateScore(string? Title, string? Composer, string? Link, int? Duration);

public class ScoreService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public ScoreService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
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

    public ReturnValue<Score[]> ListScores(bool includeMusicSheets = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListScore))
            return ErrorUtils.NotPermitted(nameof(Score), "read all");
        
        IQueryable<Score> scoresQuery = _dbContext.Scores;
        
        if  (includeMusicSheets)
            scoresQuery = scoresQuery.Include(s => s.MusicSheets);
        
        return scoresQuery.ToArray();
    }

    public ReturnValue<Score> GetScoreById(int scoreId, bool includeSheets = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListScore))
            return ErrorUtils.NotPermitted(nameof(Score), scoreId.ToString());
        
        IQueryable<Score> query = _dbContext.Scores;

        if (includeSheets)
            query = query.Include(s => s.MusicSheets);

        var score = query.FirstOrDefault(s => s.ScoreId == scoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), scoreId.ToString());

        return score;
    }

    public ReturnValue<Score> CreateScore(CreateScore createScore)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateScore))
            return ErrorUtils.NotPermitted(nameof(Score), createScore.Title);

        if (createScore.Duration <= 0)
            return ErrorUtils.ValueOutOfRange(nameof(Score), identifier: "Duration must be > 0");

        if (!IsValidHttpsLink(createScore.Link))
            return ErrorUtils.ValueNotFound(nameof(Score), identifier: "Link must start with https://");

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
        return newScore;
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
                return ErrorUtils.NotPermitted(nameof(Score), "Link must be https://");

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

        _dbContext.Scores.Remove(score);
        _dbContext.SaveChanges();
        return true;
    }
}