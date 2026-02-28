using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateScore(string Title, string Composer, string Link, int Duration);
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

    public ReturnValue<Score[]> ListScores()
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListScore))
            return ErrorUtils.NotPermitted(nameof(Score), "read all");

        return _dbContext.Scores
            .OrderBy(score => score.Title)
            .ToArray();
    }

    public ReturnValue<Score> GetScoreById(int scoreId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListScore))
            return ErrorUtils.NotPermitted(nameof(Score), scoreId.ToString());

        var score = LoadScoreById(scoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), scoreId.ToString());

        return score;
    }

    public ReturnValue<Score> CreateScore(CreateScore createScore)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateScore))
            return ErrorUtils.NotPermitted(nameof(Score), createScore.Title);

        if (createScore.Duration <= 0)
            return ErrorUtils.NotPermitted(nameof(Score), "Duration must be > 0");

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

        var newTitle = updateScore.Title ?? score.Title;
        var newComposer = updateScore.Composer ?? score.Composer;
        var newLink = updateScore.Link ?? score.Link;
        var newDuration = updateScore.Duration ?? score.Duration;

        if (newDuration <= 0)
            return ErrorUtils.NotPermitted(nameof(Score), "Duration must be > 0");

        var wouldDuplicate = _dbContext.Scores.Any(existing =>
            existing.ScoreId != scoreId &&
            existing.Title == newTitle);

        if (wouldDuplicate)
            return ErrorUtils.AlreadyExists(nameof(Score), newTitle);

        score.Title = newTitle;
        score.Composer = newComposer;
        score.Link = newLink;
        score.Duration = newDuration;

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