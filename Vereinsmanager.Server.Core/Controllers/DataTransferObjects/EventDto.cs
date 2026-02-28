#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class EventDto : MetaDataDto
{
    public int EventId { get; init; }
    public string Name { get; init; }
    public DateTime Date { get; init; }

    public EventDto(Event ev)
    {
        EventId = ev.EventId;
        Name = ev.Name;
        Date = ev.Date;

        CreatedAt = ev.CreatedAt;
        CreatedBy = ev.CreatedBy;
        UpdatedAt = ev.UpdatedAt;
        UpdatedBy = ev.UpdatedBy;
    }
}