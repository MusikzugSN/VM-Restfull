#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class AlternateVoiceDto : MetaDataDto
{
    public int AlternateVoiceId { get; init; }
    public int Alternative { get; init; }
    public int Priority { get; init; }
    public int VoiceId { get; init; }

    public AlternateVoiceDto(AlternateVoice av)
    {
        AlternateVoiceId = av.AlternateVoiceId;
        Alternative = av.Alternative;
        Priority = av.Priority;
        VoiceId = av.VoiceId;

        CreatedAt = av.CreatedAt;
        CreatedBy = av.CreatedBy;
        UpdatedAt = av.UpdatedAt;
        UpdatedBy = av.UpdatedBy;
    }
}