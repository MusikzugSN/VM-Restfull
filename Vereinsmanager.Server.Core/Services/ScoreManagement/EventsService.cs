#nullable enable
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateEvent(string Name, DateTime Date);
public record UpdateEvent(string? Name, DateTime? Date);

public record AddEventScore(int ScoreId);
public record UpdateEventScore(int? ScoreId); 

public class EventService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public EventService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }


    public Event? LoadEventByName(string name)
    {
        return _dbContext.Events.FirstOrDefault(e => e.Name == name);
    }

    public Event? LoadEventById(int eventId, bool includeEventScores = false, bool includeScores = false)
    {
        IQueryable<Event> q = _dbContext.Events;

        if (includeScores)
            q = q.Include(e => e.EventScore).ThenInclude(es => es.Score);
        else if (includeEventScores)
            q = q.Include(e => e.EventScore);

        return q.FirstOrDefault(e => e.EventId == eventId);
    }

    public EventScore? LoadEventScoreById(int eventId, int eventScoreId)
    {
        return _dbContext.EventScores
            .FirstOrDefault(es => es.EventId == eventId && es.EventScoreId == eventScoreId);
    }


    public ReturnValue<Event[]> ListEvents(bool includeEventScores = false, bool includeScores = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(Event), "read all");

        IQueryable<Event> q = _dbContext.Events;

        if (includeScores)
        {
            q = q.Include(e => e.EventScore).ThenInclude(es => es.Score);
            
        }
        else if (includeEventScores)
        {
            q = q.Include(e => e.EventScore);
        }

        return q.ToArray();
    }
    public ReturnValue<Event> GetEventById(int eventId, bool includeEventScores = false, bool includeScores = false)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        IQueryable<Event> q = _dbContext.Events;

        if (includeScores)
            q = q.Include(e => e.EventScore).ThenInclude(es => es.Score);
        else if (includeEventScores)
            q = q.Include(e => e.EventScore);

        var ev = q.FirstOrDefault(e => e.EventId == eventId);
        if (ev == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        return ev;
    }

    public ReturnValue<EventScore[]> ListEventScores(int eventId, bool includeScore = true)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(EventScore), "read all for event");

        var eventExists = _dbContext.Events.Any(e => e.EventId == eventId);
        if (!eventExists)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        IQueryable<EventScore> q = _dbContext.EventScores.Where(es => es.EventId == eventId);
        if (includeScore) q = q.Include(es => es.Score);

        return q.ToArray();
    }


    public ReturnValue<Event> CreateEvent(CreateEvent dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateEvent))
            return ErrorUtils.NotPermitted(nameof(Event), dto.Name);

        var duplicate = _dbContext.Events.Any(e => e.Name == dto.Name && e.Date == dto.Date);
        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Event), $"{dto.Name} ({dto.Date:yyyy-MM-dd})");

        var newEvent = new Event
        {
            Name = dto.Name,
            Date = dto.Date
        };

        _dbContext.Events.Add(newEvent);
        _dbContext.SaveChanges();
        return newEvent;
    }

    public ReturnValue<EventScore> AddScoreToEvent(int eventId, AddEventScore dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateEventScore))
            return ErrorUtils.NotPermitted(nameof(EventScore), eventId.ToString());

        var ev = _dbContext.Events.FirstOrDefault(e => e.EventId == eventId);
        if (ev == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == dto.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), dto.ScoreId.ToString());

        var duplicate = _dbContext.EventScores.Any(es => es.EventId == eventId && es.ScoreId == dto.ScoreId);
        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(EventScore), $"EventId={eventId}, ScoreId={dto.ScoreId}");

        var link = new EventScore
        {
            EventId = eventId,
            Event = ev,
            ScoreId = dto.ScoreId,
            Score = score
        };

        _dbContext.EventScores.Add(link);
        _dbContext.SaveChanges();
        return link;
    }


    public ReturnValue<Event> UpdateEvent(int eventId, UpdateEvent dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        var ev = _dbContext.Events.FirstOrDefault(e => e.EventId == eventId);
        if (ev == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        var newName = dto.Name ?? ev.Name;
        var newDate = dto.Date ?? ev.Date;

        var duplicate = _dbContext.Events.Any(e =>
            e.EventId != eventId &&
            e.Name == newName &&
            e.Date == newDate);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Event), $"{newName} ({newDate:yyyy-MM-dd})");

        ev.Name = newName;
        ev.Date = newDate;

        _dbContext.SaveChanges();
        return ev;
    }

    public ReturnValue<EventScore> UpdateEventScore(int eventId, int eventScoreId, UpdateEventScore dto)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateEventScore))
            return ErrorUtils.NotPermitted(nameof(EventScore), eventScoreId.ToString());

        var link = LoadEventScoreById(eventId, eventScoreId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(EventScore), eventScoreId.ToString());

        var newScoreId = dto.ScoreId ?? link.ScoreId;

        var score = _dbContext.Scores.FirstOrDefault(s => s.ScoreId == newScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), newScoreId.ToString());

        var duplicate = _dbContext.EventScores.Any(es =>
            es.EventScoreId != eventScoreId &&
            es.EventId == eventId &&
            es.ScoreId == newScoreId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(EventScore), $"EventId={eventId}, ScoreId={newScoreId}");

        link.ScoreId = newScoreId;
        link.Score = score;

        _dbContext.SaveChanges();
        return link;
    }


    public ReturnValue<bool> DeleteEvent(int eventId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        var ev = _dbContext.Events
            .Include(e => e.EventScore)
            .FirstOrDefault(e => e.EventId == eventId);

        if (ev == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        if (ev.EventScore.Count > 0)
            _dbContext.EventScores.RemoveRange(ev.EventScore);

        _dbContext.Events.Remove(ev);
        _dbContext.SaveChanges();
        return true;
    }

    public ReturnValue<bool> DeleteEventScore(int eventId, int eventScoreId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteEventScore))
            return ErrorUtils.NotPermitted(nameof(EventScore), eventScoreId.ToString());

        var link = LoadEventScoreById(eventId, eventScoreId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(EventScore), eventScoreId.ToString());

        _dbContext.EventScores.Remove(link);
        _dbContext.SaveChanges();
        return true;
    }
}