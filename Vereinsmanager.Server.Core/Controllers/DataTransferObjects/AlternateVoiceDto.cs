#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class AlternateVoiceDto : MetaDataDto
{
    public int AlternateVoiceId { get; init; }
    public int VoiceId { get; init; }
    public int Alternative { get; init; }
    public int Priority { get; init; }

    public AlternateVoiceDto(AlternateVoice alternateVoice)
    {
        AlternateVoiceId = alternateVoice.AlternateVoiceId;
        VoiceId = alternateVoice.VoiceId;
        Alternative = alternateVoice.Alternative;
        Priority = alternateVoice.Priority;

        CreatedAt = alternateVoice.CreatedAt;
        CreatedBy = alternateVoice.CreatedBy;
        UpdatedAt = alternateVoice.UpdatedAt;
        UpdatedBy = alternateVoice.UpdatedBy;
    }
}