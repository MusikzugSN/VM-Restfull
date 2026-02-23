#nullable enable
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


    public Score? LoadScoreByTitle(string title)
    {
        return _dbContext.Scores.FirstOrDefault(s => s.Title == title);
    }

    public Score? LoadScoreById(int scoreId)
    {
        return _dbContext.Scores.FirstOrDefault(s => s.ScoreId == scoreId);
    }


    public ReturnValue<Score[]> ListScores()
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListScore))
            return ErrorUtils.NotPermitted(nameof(Score), "read all");

        return _dbContext.Scores.ToArray();
    }


    public ReturnValue<Score> CreateScore(CreateScore dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateScore))
            return ErrorUtils.NotPermitted(nameof(Score), dto.Title);

        var duplicate = _dbContext.Scores.Any(s => s.Title == dto.Title);
        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Score), dto.Title);

        if (dto.Duration <= 0)
            return ErrorUtils.NotPermitted(nameof(Score), "Duration must be > 0"); 

        var newScore = new Score
        {
            Title = dto.Title,
            Composer = dto.Composer,
            Link = dto.Link,
            Duration = dto.Duration
        };

        _dbContext.Scores.Add(newScore);
        _dbContext.SaveChanges();
        return newScore;
    }

    public ReturnValue<Score> UpdateScore(int scoreId, UpdateScore dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateScore))
            return ErrorUtils.NotPermitted(nameof(Score), scoreId.ToString());

        var score = LoadScoreById(scoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), scoreId.ToString());

        var newTitle = dto.Title ?? score.Title;
        var newComposer = dto.Composer ?? score.Composer;
        var newLink = dto.Link ?? score.Link;
        var newDuration = dto.Duration ?? score.Duration;

        if (newDuration <= 0)
            return ErrorUtils.NotPermitted(nameof(Score), "Duration must be > 0");

        var wouldDuplicate = _dbContext.Scores.Any(s =>
            s.ScoreId != scoreId &&
            s.Title == newTitle);

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

        var hasSheets = _dbContext.MusicSheets.Any(ms => ms.ScoreId == scoreId);
        if (hasSheets)
            return ErrorUtils.NotPermitted(nameof(Score), "delete (has MusicSheets)");

        _dbContext.Scores.Remove(score);
        _dbContext.SaveChanges();
        return true;
    }
}
