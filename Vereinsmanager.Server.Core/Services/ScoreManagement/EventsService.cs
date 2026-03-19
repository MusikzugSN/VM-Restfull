using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateEvent(int GroupId, string Name, DateTime Date, bool? ShowInMyArea, List<UpdateEventScore>? Scores);
public record UpdateEvent(int? GroupId, string? Name, DateTime? Date, bool? ShowInMyArea, List<UpdateEventScore>? Scores);
public record UpdateEventScore(int ScoreId, bool? Deleted);

public class EventsService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public EventsService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    public List<Event> GetEventsByName(HashSet<string?> names)
    {
        names = names.Select(name => name?.Trim()).Select(name => name?.ToLower()).ToHashSet();
        return _dbContext.Events.Where(eventItem => names.Contains(eventItem.Name.ToLower())).ToList();
    }

    public ReturnValue<Event[]> ListEvents()
    {
        return ListEvents(false);
    }

    public ReturnValue<Event[]> ListEvents(bool includeScores)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(Event), "read all");

        return BuildEventQuery(includeScores).ToArray();
    }

    public ReturnValue<Event[]> ListEventsForMyAreas(bool includeScores)
    {
        var events = BuildEventQuery(includeScores)
            .Where(x => x.ShowInMyArea)
            .ToArray();

        var permissionFilteredEvents = events
            .Where(x => _permissionServiceLazy.Value.HasPermission(PermissionType.OpenMyNotes, x.GroupId))
            .ToArray();

        return permissionFilteredEvents;
    }

    public ReturnValue<Event> GetEventById(int eventId)
    {
        return GetEventById(eventId, false);
    }

    public ReturnValue<Event> GetEventById(int eventId, bool includeScores)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        var loadedEvent = BuildEventQuery(includeScores)
            .FirstOrDefault(eventItem => eventItem.EventId == eventId);

        if (loadedEvent == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        return loadedEvent;
    }

    public ReturnValue<Event> CreateEvent(CreateEvent createEvent)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateEvent))
            return ErrorUtils.NotPermitted(nameof(Event), createEvent.Name);

        var group = _dbContext.Groups.FirstOrDefault(groupItem => groupItem.GroupId == createEvent.GroupId);
        if (group == null)
            return ErrorUtils.ValueNotFound(nameof(Group), createEvent.GroupId.ToString());

        var duplicate = _dbContext.Events.Any(eventItem =>
            eventItem.GroupId == createEvent.GroupId &&
            eventItem.Name == createEvent.Name &&
            eventItem.Date == createEvent.Date);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Event), $"{createEvent.Name} ({createEvent.Date:O}, GroupId={createEvent.GroupId})");

        var eventToCreate = new Event
        {
            GroupId = createEvent.GroupId,
            Name = createEvent.Name,
            Date = createEvent.Date,
            ShowInMyArea = createEvent.ShowInMyArea ?? false
        };

        _dbContext.Events.Add(eventToCreate);
        _dbContext.SaveChanges();

        if (createEvent.Scores is { Count: > 0 })
        {
            var updateScoresToEventResult = UpdateScoresToEvent(eventToCreate, createEvent.Scores);
            if (!updateScoresToEventResult.IsSuccessful())
                return updateScoresToEventResult;

            _dbContext.SaveChanges();
        }

        return eventToCreate;
    }

    public ReturnValue<Event> UpdateEvent(int eventId, UpdateEvent updateEvent)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        var loadedEvent = _dbContext.Events.FirstOrDefault(eventItem => eventItem.EventId == eventId);
        if (loadedEvent == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        var newName = updateEvent.Name ?? loadedEvent.Name;
        var newDate = updateEvent.Date ?? loadedEvent.Date;
        var newGroupId = updateEvent.GroupId ?? loadedEvent.GroupId;
        var showInMyArea = updateEvent.ShowInMyArea ?? loadedEvent.ShowInMyArea;

        var groupExists = _dbContext.Groups.Any(groupItem => groupItem.GroupId == newGroupId);
        if (!groupExists)
            return ErrorUtils.ValueNotFound(nameof(Group), newGroupId.ToString());

        var duplicate = _dbContext.Events.Any(eventItem =>
            eventItem.EventId != eventId &&
            eventItem.GroupId == newGroupId &&
            eventItem.Name == newName &&
            eventItem.Date == newDate);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(Event), $"{newName} ({newDate:O}, GroupId={newGroupId})");

        loadedEvent.Name = newName;
        loadedEvent.Date = newDate;
        loadedEvent.GroupId = newGroupId;
        loadedEvent.ShowInMyArea = showInMyArea;

        if (updateEvent.Scores != null)
        {
            var updateScoresToEventResult = UpdateScoresToEvent(loadedEvent, updateEvent.Scores);
            if (!updateScoresToEventResult.IsSuccessful())
                return updateScoresToEventResult;
        }

        _dbContext.SaveChanges();
        return loadedEvent;
    }

    public ReturnValue<bool> DeleteEvent(int eventId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        var loadedEvent = _dbContext.Events.FirstOrDefault(eventItem => eventItem.EventId == eventId);
        if (loadedEvent == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        var links = _dbContext.EventScores
            .Where(link => link.EventId == eventId)
            .ToList();

        if (links.Count > 0)
            _dbContext.EventScores.RemoveRange(links);

        _dbContext.Events.Remove(loadedEvent);
        _dbContext.SaveChanges();
        return true;
    }

    public ReturnValue<EventScore[]> ListScoresInEvent(int eventId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(EventScore), "read all for event");

        var eventExists = _dbContext.Events.Any(eventItem => eventItem.EventId == eventId);
        if (!eventExists)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        IQueryable<EventScore> query = _dbContext.EventScores
            .Where(link => link.EventId == eventId)
            .Include(link => link.Score);

        return query.ToArray();
    }

    public ReturnValue<EventScore> GetEventScoreById(int eventScoreId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(EventScore), eventScoreId.ToString());

        var link = _dbContext.EventScores
            .Include(linkItem => linkItem.Score)
            .Include(linkItem => linkItem.Event)
            .FirstOrDefault(linkItem => linkItem.EventScoreId == eventScoreId);

        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(EventScore), eventScoreId.ToString());

        return link;
    }

    public ReturnValue<EventScore> AddScoreToEvent(int eventId, UpdateEventScore updateEventScore)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateEvent))
            return ErrorUtils.NotPermitted(nameof(EventScore), eventId.ToString());

        var loadedEvent = _dbContext.Events.FirstOrDefault(eventItem => eventItem.EventId == eventId);
        if (loadedEvent == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        var score = _dbContext.Scores.FirstOrDefault(scoreItem => scoreItem.ScoreId == updateEventScore.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), updateEventScore.ScoreId.ToString());

        var duplicate = _dbContext.EventScores.Any(link =>
            link.EventId == eventId &&
            link.ScoreId == updateEventScore.ScoreId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(EventScore), $"EventId={eventId}, ScoreId={updateEventScore.ScoreId}");

        var linkToCreate = new EventScore
        {
            EventId = eventId,
            Event = loadedEvent,
            ScoreId = updateEventScore.ScoreId,
            Score = score
        };

        _dbContext.EventScores.Add(linkToCreate);
        _dbContext.SaveChanges();
        return linkToCreate;
    }

    public ReturnValue<EventScore> UpdateEventScore(int eventScoreId, UpdateEventScore updateEventScore)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateEvent))
            return ErrorUtils.NotPermitted(nameof(EventScore), eventScoreId.ToString());

        var link = _dbContext.EventScores.FirstOrDefault(linkItem => linkItem.EventScoreId == eventScoreId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(EventScore), eventScoreId.ToString());

        var score = _dbContext.Scores.FirstOrDefault(scoreItem => scoreItem.ScoreId == updateEventScore.ScoreId);
        if (score == null)
            return ErrorUtils.ValueNotFound(nameof(Score), updateEventScore.ScoreId.ToString());

        var duplicate = _dbContext.EventScores.Any(linkItem =>
            linkItem.EventScoreId != eventScoreId &&
            linkItem.EventId == link.EventId &&
            linkItem.ScoreId == updateEventScore.ScoreId);

        if (duplicate)
            return ErrorUtils.AlreadyExists(nameof(EventScore), $"EventId={link.EventId}, ScoreId={updateEventScore.ScoreId}");

        link.ScoreId = updateEventScore.ScoreId;
        link.Score = score;

        _dbContext.SaveChanges();
        return link;
    }

    public ReturnValue<bool> DeleteEventScore(int eventScoreId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteEvent))
            return ErrorUtils.NotPermitted(nameof(EventScore), eventScoreId.ToString());

        var link = _dbContext.EventScores.FirstOrDefault(linkItem => linkItem.EventScoreId == eventScoreId);
        if (link == null)
            return ErrorUtils.ValueNotFound(nameof(EventScore), eventScoreId.ToString());

        _dbContext.EventScores.Remove(link);
        _dbContext.SaveChanges();
        return true;
    }

    private ReturnValue<Event> UpdateScoresToEvent(Event eventEntity, List<UpdateEventScore> incoming)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(Event), "read all");

        var normalized = incoming
            .GroupBy(x => x.ScoreId)
            .Select(group => group.Last())
            .ToList();

        var idsToDeleted = normalized
            .Where(x => x.Deleted ?? false)
            .Select(x => x.ScoreId)
            .ToHashSet();

        var entriesToDelete = _dbContext.EventScores
            .Where(es => es.EventId == eventEntity.EventId)
            .Where(es => idsToDeleted.Contains(es.ScoreId))
            .ToList();

        _dbContext.RemoveRange(entriesToDelete);

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

        var createReferences = existingScores
            .Select(CreateViaScoreId)
            .ToList();

        _dbContext.AddRange(createReferences);
        _dbContext.SaveChanges();

        return eventEntity;

        EventScore CreateViaScoreId(Score score)
        {
            return new EventScore
            {
                Event = eventEntity,
                Score = score
            };
        }
    }

    private IQueryable<Event> BuildEventQuery(bool includeScores)
    {
        IQueryable<Event> query = _dbContext.Events;

        if (includeScores)
            query = query.Include(eventItem => eventItem.EventScore);

        return query;
    }
}