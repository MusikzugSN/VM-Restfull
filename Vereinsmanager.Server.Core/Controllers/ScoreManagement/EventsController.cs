using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/event")]
public class EventsController : ControllerBase
{
    [HttpGet]
    public ActionResult<EventDto[]> GetEvents(
        [FromQuery] bool includeEventScores,
        [FromQuery] bool includeScores,
        [FromServices] EventService eventService)
    {
        if (includeScores && !includeEventScores)
        {
            return BadRequest("includeScores kann nur zusammen mit includeEventScores genutzt werden.");
        }

        var eventsResult = eventService.ListEvents(includeEventScores, includeScores);

        if (eventsResult.IsSuccessful())
        {
            var events = eventsResult.GetValue();
            return events.Select(e => new EventDto(e)).ToArray();
        }

        return (ObjectResult)eventsResult;
    }

    [HttpGet]
    [Route("{eventId:int}")]
    public ActionResult<EventDto> GetEventById(
        [FromRoute] int eventId,
        [FromQuery] bool includeEventScores,
        [FromQuery] bool includeScores,
        [FromServices] EventService eventService)
    {
        if (includeScores && !includeEventScores)
        {
            return BadRequest("includeScores kann nur zusammen mit includeEventScores genutzt werden.");
        }

        var eventResult = eventService.GetEventById(eventId, includeEventScores, includeScores);

        if (eventResult.IsSuccessful())
        {
            var loadedEvent = eventResult.GetValue();
            return new EventDto(loadedEvent);
        }

        return (ObjectResult)eventResult;
    }

    [HttpPost]
    public ActionResult<EventDto> CreateEvent(
        [FromBody] CreateEvent createEvent,
        [FromServices] EventService eventService)
    {
        var createdResult = eventService.CreateEvent(createEvent);

        if (createdResult.IsSuccessful())
        {
            return new EventDto(createdResult.GetValue());
        }

        return (ObjectResult)createdResult;
    }

    [HttpPatch]
    [Route("{eventId:int}")]
    public ActionResult<EventDto> UpdateEvent(
        [FromRoute] int eventId,
        [FromBody] UpdateEvent updateEvent,
        [FromServices] EventService eventService)
    {
        var updatedResult = eventService.UpdateEvent(eventId, updateEvent);

        if (updatedResult.IsSuccessful())
        {
            return new EventDto(updatedResult.GetValue());
        }

        return (ObjectResult)updatedResult;
    }

    [HttpDelete]
    [Route("{eventId:int}")]
    public ActionResult<bool> DeleteEvent(
        [FromRoute] int eventId,
        [FromServices] EventService eventService)
    {
        var deletedResult = eventService.DeleteEvent(eventId);

        if (deletedResult.IsSuccessful())
        {
            return deletedResult.GetValue();
        }

        return (ObjectResult)deletedResult;
    }
}