#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class EventScoreDto : MetaDataDto
{
    public int EventScoreId { get; init; }
    public int EventId { get; init; }
    public int ScoreId { get; init; }

    public EventScoreDto(EventScore es)
    {
        EventScoreId = es.EventScoreId;
        EventId = es.EventId;
        ScoreId = es.ScoreId;

        CreatedAt = es.CreatedAt;
        CreatedBy = es.CreatedBy;
        UpdatedAt = es.UpdatedAt;
        UpdatedBy = es.UpdatedBy;
    }
}