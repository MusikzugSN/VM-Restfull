using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class EventSheetDto
{
    public int ScoreId { get; init; }
}

public class EventDto : MetaDataDto
{
    public int EventId { get; init; }
    public int GroupId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public bool ShowInMyArea { get; init; }

    public EventSheetDto[] Scores { get; init; } = Array.Empty<EventSheetDto>();

    public EventDto(Event eventModel)
    {
        EventId = eventModel.EventId;
        GroupId = eventModel.GroupId;
        Name = eventModel.Name;
        Date = eventModel.Date;
        ShowInMyArea = eventModel.ShowInMyArea;

        Scores = eventModel.EventScore?
                     .Select(x => new EventSheetDto
                     {
                         ScoreId = x.ScoreId
                     })
                     .ToArray()
                 ?? Array.Empty<EventSheetDto>();

        CreatedAt = eventModel.CreatedAt;
        CreatedBy = eventModel.CreatedBy;
        UpdatedAt = eventModel.UpdatedAt;
        UpdatedBy = eventModel.UpdatedBy;
    }
}
