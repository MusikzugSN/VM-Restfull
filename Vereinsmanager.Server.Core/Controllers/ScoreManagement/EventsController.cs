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
        [FromQuery] bool includeScores,
        [FromServices] EventsService eventsService)
    {
        var eventsResult = eventsService.ListEvents(includeScores);

        if (eventsResult.IsSuccessful())
        {
            return eventsResult.GetValue()!
                .Select(eventItem => new EventDto(eventItem))
                .ToArray();
        }

        return (ObjectResult)eventsResult;
    }

    [HttpGet("forMyArea")]
    public ActionResult<EventDto[]> GetMyEvents(
        [FromQuery] bool includeScores,
        [FromServices] EventsService eventsService)
    {
        var eventsResult = eventsService.ListEventsForMyAreas(includeScores);

        if (eventsResult.IsSuccessful())
        {
            return eventsResult.GetValue()!
                .Select(eventItem => new EventDto(eventItem))
                .ToArray();
        }

        return (ObjectResult)eventsResult;
    }

    [HttpGet("{eventId:int}")]
    public ActionResult<EventDto> GetEventById(
        [FromRoute] int eventId,
        [FromQuery] bool includeScores,
        [FromServices] EventsService eventsService)
    {
        var eventResult = eventsService.GetEventById(eventId, includeScores);

        if (eventResult.IsSuccessful())
        {
            return new EventDto(eventResult.GetValue()!);
        }

        return (ObjectResult)eventResult;
    }

    [HttpPost]
    public ActionResult<EventDto> CreateEvent(
        [FromBody] CreateEvent createEvent,
        [FromServices] EventsService eventsService)
    {
        var createdResult = eventsService.CreateEvent(createEvent);

        if (createdResult.IsSuccessful())
        {
            return new EventDto(createdResult.GetValue()!);
        }

        return (ObjectResult)createdResult;
    }

    [HttpPatch("{eventId:int}")]
    public ActionResult<EventDto> UpdateEvent(
        [FromRoute] int eventId,
        [FromBody] UpdateEvent updateEvent,
        [FromServices] EventsService eventsService)
    {
        var updatedResult = eventsService.UpdateEvent(eventId, updateEvent);

        if (updatedResult.IsSuccessful())
        {
            return new EventDto(updatedResult.GetValue()!);
        }

        return (ObjectResult)updatedResult;
    }

    [HttpDelete("{eventId:int}")]
    public ActionResult<bool> DeleteEvent(
        [FromRoute] int eventId,
        [FromServices] EventsService eventsService)
    {
        var deletedResult = eventsService.DeleteEvent(eventId);

        if (deletedResult.IsSuccessful())
        {
            return deletedResult.GetValue();
        }

        return (ObjectResult)deletedResult;
    }
}