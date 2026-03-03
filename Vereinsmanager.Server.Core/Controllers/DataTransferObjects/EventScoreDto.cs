#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class EventScoreDto : MetaDataDto
{
    public int EventId { get; init; }
    public int ScoreId { get; init; }

    public bool Deleted { get; init; }

    public EventScoreDto(EventScore eventScore)
    {
        EventId = eventScore.EventId;
        ScoreId = eventScore.ScoreId;

        CreatedAt = eventScore.CreatedAt;
        CreatedBy = eventScore.CreatedBy;
        UpdatedAt = eventScore.UpdatedAt;
        UpdatedBy = eventScore.UpdatedBy;
    }
}