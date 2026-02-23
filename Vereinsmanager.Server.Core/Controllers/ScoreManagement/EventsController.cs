#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Services.ScoreManagement;
using Vereinsmanager.Controllers.DataTransferObjects;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/event")]
public class EventController : ControllerBase
{
    [HttpGet]
    public ActionResult<EventDto[]> GetEvents(
        [FromQuery] bool includeEventScores,
        [FromQuery] bool includeScores,
        [FromServices] EventService eventService)
    {
        var events = eventService.ListEvents(includeEventScores, includeScores);

        if (events.IsSuccessful())
        {
            return events.GetValue()!
                .Select(e => new EventDto(e))
                .ToArray();
        }

        return (ObjectResult)events;
    }
    
    [HttpGet]
    [Route("{eventId:int}")]
    public ActionResult<EventDto> GetEventById(
        [FromRoute] int eventId,
        [FromQuery] bool includeEventScores,
        [FromQuery] bool includeScores,
        [FromServices] EventService eventService)
    {
        var ev = eventService.GetEventById(eventId, includeEventScores, includeScores);

        if (ev.IsSuccessful())
        {
            return new EventDto(ev.GetValue()!);
        }

        return (ObjectResult)ev;
    }
    
    [HttpPost]
    public ActionResult<EventDto> CreateEvent(
        [FromBody] CreateEvent createEvent,
        [FromServices] EventService eventService)
    {
        var created = eventService.CreateEvent(createEvent);

        if (created.IsSuccessful())
        {
            return new EventDto(created.GetValue()!);
        }

        return (ObjectResult)created;
    }

    [HttpPatch]
    [Route("{eventId:int}")]
    public ActionResult<EventDto> UpdateEvent(
        [FromRoute] int eventId,
        [FromBody] UpdateEvent updateEvent,
        [FromServices] EventService eventService)
    {
        var updated = eventService.UpdateEvent(eventId, updateEvent);

        if (updated.IsSuccessful())
        {
            return new EventDto(updated.GetValue()!);
        }

        return (ObjectResult)updated;
    }

    [HttpDelete]
    [Route("{eventId:int}")]
    public ActionResult<bool> DeleteEvent(
        [FromRoute] int eventId,
        [FromServices] EventService eventService)
    {
        var deleted = eventService.DeleteEvent(eventId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }


    [HttpGet]
    [Route("{eventId:int}/scores")]
    public ActionResult<EventScoreDto[]> ListEventScores(
        [FromRoute] int eventId,
        [FromQuery] bool includeScore,
        [FromServices] EventService eventService)
    {
        var scores = eventService.ListEventScores(eventId, includeScore);

        if (scores.IsSuccessful())
        {
            return scores.GetValue()!
                .Select(es => new EventScoreDto(es))
                .ToArray();
        }

        return (ObjectResult)scores;
    }

    [HttpPost]
    [Route("{eventId:int}/scores")]
    public ActionResult<EventScoreDto> AddScoreToEvent(
        [FromRoute] int eventId,
        [FromBody] AddEventScore addEventScore,
        [FromServices] EventService eventService)
    {
        var added = eventService.AddScoreToEvent(eventId, addEventScore);

        if (added.IsSuccessful())
        {
            return new EventScoreDto(added.GetValue()!);
        }

        return (ObjectResult)added;
    }

    [HttpPatch]
    [Route("{eventId:int}/scores/{eventScoreId:int}")]
    public ActionResult<EventScoreDto> UpdateEventScore(
        [FromRoute] int eventId,
        [FromRoute] int eventScoreId,
        [FromBody] UpdateEventScore updateEventScore,
        [FromServices] EventService eventService)
    {
        var updated = eventService.UpdateEventScore(eventId, eventScoreId, updateEventScore);

        if (updated.IsSuccessful())
        {
            return new EventScoreDto(updated.GetValue()!);
        }

        return (ObjectResult)updated;
    }

    [HttpDelete]
    [Route("{eventId:int}/scores/{eventScoreId:int}")]
    public ActionResult<bool> DeleteEventScore(
        [FromRoute] int eventId,
        [FromRoute] int eventScoreId,
        [FromServices] EventService eventService)
    {
        var deleted = eventService.DeleteEventScore(eventId, eventScoreId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }
}