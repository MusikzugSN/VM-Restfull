using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record AddEventScore(int ScoreId);
public record UpdateEventScore(int ScoreId);
public record UpdateEventScoreItem(int? EventScoreId, UpdateEventScore Data);
public record CreateEvent(string Name, DateTime Date, List<AddEventScore>? Scores);
public record UpdateEvent(string? Name, DateTime? Date, List<UpdateEventScoreItem>? Scores);

public class EventService
{
    private readonly ServerDatabaseContext _dbContext;

    public EventService(ServerDatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ReturnValue<Event[]> ListEvents(bool includeEventScores, bool includeScores)
    {
        IQueryable<Event> query = _dbContext.Events;

        if (includeScores && !includeEventScores)
        {
            includeEventScores = true;
        }

        if (includeEventScores)
        {
            query = query.Include(e => e.EventScore);

            if (includeScores)
            {
                query = query.Include(e => e.EventScore)
                             .ThenInclude(es => es.Score);
            }
        }

        return query.ToArray();
    }

    public ReturnValue<Event> GetEventById(int eventId, bool includeEventScores, bool includeScores)
    {
        IQueryable<Event> query = _dbContext.Events;

        if (includeScores && !includeEventScores)
        {
            includeEventScores = true;
        }

        if (includeEventScores)
        {
            query = query.Include(e => e.EventScore);

            if (includeScores)
            {
                query = query.Include(e => e.EventScore)
                             .ThenInclude(es => es.Score);
            }
        }

        var loadedEvent = query.FirstOrDefault(e => e.EventId == eventId);
        if (loadedEvent == null)
        {
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());
        }

        return loadedEvent;
    }

    public ReturnValue<Event> CreateEvent(CreateEvent createEvent)
    {
        var duplicateExists = _dbContext.Events.Any(e => e.Name == createEvent.Name && e.Date == createEvent.Date);
        if (duplicateExists)
        {
            return ErrorUtils.AlreadyExists(nameof(Event), $"{createEvent.Name} {createEvent.Date:O}");
        }

        List<Score>? scoresToAttach = null;

        if (createEvent.Scores != null && createEvent.Scores.Count > 0)
        {
            scoresToAttach = new List<Score>(createEvent.Scores.Count);

            foreach (var scoreRef in createEvent.Scores)
            {
                var score = _dbContext.Scores.FirstOrDefault(x => x.ScoreId == scoreRef.ScoreId);
                if (score == null)
                {
                    return ErrorUtils.ValueNotFound(nameof(Score), scoreRef.ScoreId.ToString());
                }

                scoresToAttach.Add(score);
            }
        }

        var newEvent = new Event
        {
            Name = createEvent.Name,
            Date = createEvent.Date
        };

        if (scoresToAttach != null)
        {
            foreach (var score in scoresToAttach)
            {
                newEvent.EventScore.Add(new EventScore
                {
                    Event = newEvent,
                    Score = score,
                    ScoreId = score.ScoreId
                });
            }
        }

        _dbContext.Events.Add(newEvent);
        _dbContext.SaveChanges();

        return newEvent;
    }

    public ReturnValue<Event> UpdateEvent(int eventId, UpdateEvent updateEvent)
    {
        var loadedEvent = _dbContext.Events
            .Include(e => e.EventScore)
            .FirstOrDefault(e => e.EventId == eventId);

        if (loadedEvent == null)
        {
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());
        }

        if (updateEvent.Name != null)
        {
            loadedEvent.Name = updateEvent.Name;
        }

        if (updateEvent.Date != null)
        {
            loadedEvent.Date = updateEvent.Date.Value;
        }

        if (updateEvent.Scores != null)
        {
            var existingById = loadedEvent.EventScore.ToDictionary(es => es.EventScoreId);

            var incomingExistingIds = updateEvent.Scores
                .Where(x => x.EventScoreId.HasValue)
                .Select(x => x.EventScoreId.Value)
                .ToHashSet();

            var toDelete = loadedEvent.EventScore
                .Where(es => !incomingExistingIds.Contains(es.EventScoreId))
                .ToList();

            _dbContext.Set<EventScore>().RemoveRange(toDelete);

            foreach (var item in updateEvent.Scores)
            {
                var score = _dbContext.Scores.FirstOrDefault(x => x.ScoreId == item.Data.ScoreId);
                if (score == null)
                {
                    return ErrorUtils.ValueNotFound(nameof(Score), item.Data.ScoreId.ToString());
                }

                if (item.EventScoreId.HasValue)
                {
                    if (!existingById.TryGetValue(item.EventScoreId.Value, out var existing))
                    {
                        return ErrorUtils.ValueNotFound(nameof(EventScore), item.EventScoreId.Value.ToString());
                    }

                    existing.Score = score;
                    existing.ScoreId = score.ScoreId;
                }
                else
                {
                    loadedEvent.EventScore.Add(new EventScore
                    {
                        Event = loadedEvent,
                        Score = score,
                        ScoreId = score.ScoreId
                    });
                }
            }
        }

        _dbContext.SaveChanges();
        return loadedEvent;
    }

    public ReturnValue<bool> DeleteEvent(int eventId)
    {
        var loadedEvent = _dbContext.Events.FirstOrDefault(e => e.EventId == eventId);
        if (loadedEvent == null)
        {
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());
        }

        _dbContext.Events.Remove(loadedEvent);
        _dbContext.SaveChanges();
        return true;
    }
}