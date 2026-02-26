using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class VoiceDto : MetaDataDto
{
    public int VoiceId { get; init; }
    public string Name { get; init; }

    public VoiceDto(Voice voice)
    {
        VoiceId = voice.VoiceId;
        Name = voice.Name;

        CreatedAt = voice.CreatedAt;
        CreatedBy = voice.CreatedBy;
        UpdatedAt = voice.UpdatedAt;
        UpdatedBy = voice.UpdatedBy;
    }
}