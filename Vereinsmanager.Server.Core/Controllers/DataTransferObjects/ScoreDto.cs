using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class ScoreDto : MetaDataDto
{
    public int ScoreId { get; init; }
    public string Title { get; init; }
    public string Composer { get; init; }
    public string Link { get; init; }
    public int Duration { get; init; }

    public ScoreDto(Score score)
    {
        ScoreId = score.ScoreId;
        Title = score.Title;
        Composer = score.Composer;
        Link = score.Link;
        Duration = score.Duration;

        CreatedAt = score.CreatedAt;
        CreatedBy = score.CreatedBy;
        UpdatedAt = score.UpdatedAt;
        UpdatedBy = score.UpdatedBy;
    }
}