using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.ScoreManagment;
using Vereinsmanager.Utils;
using Vereinsmanager.Services.Models;

namespace Vereinsmanager.Services.ScoreManagement;

public record CreateEvent(string Name, DateTime Date, bool? ShowInMyArea, List<UpdateEventScore>? Scores);
public record UpdateEventScore(int ScoreId, bool? Deleted);
public record UpdateEvent(string? Name, DateTime? Date, bool? ShowInMyArea, List<UpdateEventScore>? Scores);

public class EventService
{
    private readonly ServerDatabaseContext _dbContext;
    private readonly Lazy<PermissionService> _permissionServiceLazy;

    public EventService(ServerDatabaseContext dbContext, Lazy<PermissionService> permissionServiceLazy)
    {
        _dbContext = dbContext;
        _permissionServiceLazy = permissionServiceLazy;
    }

    private IQueryable<Event> BuildEventQuery(bool includeScores = false)
    {
        var query = _dbContext.Events;
        
        if (includeScores)
        {
            query.Include(es => es.EventScore);
        }

        return query;
    }

    private Event? LoadEventEntityById(int eventId)
    {
        return BuildEventQuery().FirstOrDefault(e => e.EventId == eventId);
    }

    private void UpdateScoresToEvent(Event eventEntity, List<UpdateEventScore> incoming)
    {
        var normalized = incoming
            .GroupBy(x => x.ScoreId)
            .Select(g => g.Last())
            .ToList();

        //Delete Values
        var idsToDeleted = normalized
            .Where(x => x.Deleted ?? false)
            .Select(x => x.ScoreId)
            .ToHashSet();
        
        var entrysToDelete = _dbContext.EventScores
            .Where(es => es.EventId == eventEntity.EventId)
            .Where(es => idsToDeleted.Contains(es.ScoreId))
            .ToList();

        _dbContext.RemoveRange(entrysToDelete);
        
        //Add Values
        var idsToCreate = normalized
            .Where(x => (x.Deleted ?? false) == false)
            .Select(x => x.ScoreId)
            .ToHashSet();

        var existingScores = _dbContext.Scores
            .Where(x => idsToCreate.Contains(x.ScoreId))
            .ToHashSet();

        var createReferences = existingScores
            .Select(CreateViaScoreId)
            .ToList();

        _dbContext.AddRange(createReferences);
        _dbContext.SaveChanges();

        return;
        EventScore CreateViaScoreId(Score score)
        {
            return new EventScore
            {
                Event = eventEntity,
                Score = score
            };
        }
    }

    public ReturnValue<Event[]> ListEvents(bool includeScores)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(Event), "read all");

        return BuildEventQuery(includeScores).ToArray();
    }
    
    public ReturnValue<Event[]> ListEventsForMyAreas()
    {
        var folders = _dbContext.Events.Where(x => x.ShowInMyArea).ToArray();
        var permissionFilteredFolders = folders.Where(x => _permissionServiceLazy.Value.HasPermission(PermissionType.OpenMyNotes, x.GroupId)).ToArray();
        
        return permissionFilteredFolders;
    }

    public ReturnValue<Event> GetEventById(int eventId, bool includeScores)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.ListEvent))
            return ErrorUtils.NotPermitted(nameof(Event), "read all");
        
        var loadedEvent = BuildEventQuery(includeScores).FirstOrDefault(e => e.EventId == eventId);
        if (loadedEvent == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        return loadedEvent;
    }

    public ReturnValue<Event> CreateEvent(CreateEvent createEvent)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.CreateEvent))
            return ErrorUtils.NotPermitted(nameof(Event), createEvent.Name);

        var duplicateExists = BuildEventQuery().Any(e => e.Name == createEvent.Name && e.Date == createEvent.Date);
        if (duplicateExists)
            return ErrorUtils.AlreadyExists(nameof(Event), $"{createEvent.Name} {createEvent.Date:O}");

       
        var newEvent = new Event
        {
            Name = createEvent.Name,
            Date = createEvent.Date,
            ShowInMyArea = createEvent.ShowInMyArea ?? false,
        };

        if (createEvent.Scores?.Any() ?? false)
        {
            UpdateScoresToEvent(newEvent, createEvent.Scores);
        }

        _dbContext.Events.Add(newEvent);
        _dbContext.SaveChanges();
        return newEvent;
    }

    public ReturnValue<Event> UpdateEvent(int eventId, UpdateEvent updateEvent)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.UpdateEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        var loadedEvent = LoadEventEntityById(eventId);
        if (loadedEvent == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        if (updateEvent.Name != null)
            loadedEvent.Name = updateEvent.Name;

        if (updateEvent.Date != null)
            loadedEvent.Date = updateEvent.Date.Value;
        
        if (updateEvent.ShowInMyArea != null)
            loadedEvent.ShowInMyArea = updateEvent.ShowInMyArea ?? false;

        if (updateEvent.Scores?.Any() ?? false)
        {
            UpdateScoresToEvent(loadedEvent, updateEvent.Scores);
        }

        _dbContext.SaveChanges();
        return loadedEvent;
    }

    public ReturnValue<bool> DeleteEvent(int eventId)
    {
        if (!_permissionServiceLazy.Value.HasPermission(PermissionType.DeleteEvent))
            return ErrorUtils.NotPermitted(nameof(Event), eventId.ToString());

        var loadedEvent = LoadEventEntityById(eventId);
        if (loadedEvent == null)
            return ErrorUtils.ValueNotFound(nameof(Event), eventId.ToString());

        _dbContext.Events.Remove(loadedEvent);
        _dbContext.SaveChanges();
        return true;
    }
}