#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class AlternateVoiceDto : MetaDataDto
{
    public int AlternateVoiceId { get; init; }
    public int Priority { get; init; }
    public int VoiceId { get; init; }
    public int AlternativeVoiceId { get; init; }

    public AlternateVoiceDto(AlternateVoice av)
    {
        AlternateVoiceId = av.AlternateVoiceId;
        Priority = av.Priority;
        VoiceId = av.VoiceId;
        AlternativeVoiceId = av.AlternativeVoiceId;

        CreatedAt = av.CreatedAt;
        CreatedBy = av.CreatedBy;
        UpdatedAt = av.UpdatedAt;
        UpdatedBy = av.UpdatedBy;
    }
}