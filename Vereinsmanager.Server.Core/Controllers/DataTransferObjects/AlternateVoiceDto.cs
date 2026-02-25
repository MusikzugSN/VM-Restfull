#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class AlternateVoiceDto : MetaDataDto
{
    public int AlternateVoiceId { get; init; }
    public string Alternative { get; init; }
    public int Priority { get; init; }
    public int VoiceId { get; init; }

    public AlternateVoiceDto(AlternateVoice av)
    {
        AlternateVoiceId = av.AlternateVoiceId;
        Alternative = av.Alternative;
        Priority = av.Priority;
        VoiceId = av.VoiceId;
    }
}