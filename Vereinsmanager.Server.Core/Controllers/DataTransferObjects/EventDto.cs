using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class EventDto : MetaDataDto
{
    public int EventId { get; init; }
    public string Name { get; init; }
    public DateTime Date { get; init; }
    public int GroupId { get; init; }
    
    public List<int> ScoreIds { get; init; }

    public EventDto(Event eventModel)
    {
        EventId = eventModel.EventId;
        Name = eventModel.Name;
        Date = eventModel.Date;
        GroupId = eventModel.GroupId;
        
        ScoreIds = eventModel.EventScore?
                       .Select(es => es.ScoreId)
                       .ToList()
                   ?? new List<int>();
        
        CreatedAt = eventModel.CreatedAt;
        CreatedBy = eventModel.CreatedBy;
        UpdatedAt = eventModel.UpdatedAt;
        UpdatedBy = eventModel.UpdatedBy;
    }
}