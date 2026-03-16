using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class EventDto : MetaDataDto
{
    public int EventId { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int GroupId { get; set; }
    public int[] ScoreIds { get; set; }

    public EventDto(Event eventModel)
    {
        EventId = eventModel.EventId;
        Name = eventModel.Name;
        Date = eventModel.Date;
        GroupId = eventModel.GroupId;

        ScoreIds = eventModel.EventScore?
                       .Select(es => es.ScoreId)
                       .ToArray()
                   ?? [];

        CreatedAt = eventModel.CreatedAt;
        CreatedBy = eventModel.CreatedBy;
        UpdatedAt = eventModel.UpdatedAt;
        UpdatedBy = eventModel.UpdatedBy;
    }
}
