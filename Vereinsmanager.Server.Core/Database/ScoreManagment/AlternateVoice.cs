using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(VoiceId), nameof(Alternative), IsUnique = true)]
[Index(nameof(VoiceId), nameof(Priority), IsUnique = true)]
public class AlternateVoice : MetaData
{
    [Key]
    public int AlternateVoiceId { get; set; }

    [Required]
    public int Priority { get; set; }

    [Required]
    public int VoiceId { get; set; }

    [ForeignKey(nameof(VoiceId))]
    [InverseProperty(nameof(Voice.AlternateVoices))]
    public required Voice Voice { get; set; }

    [Required]
    public int Alternative { get; set; }

    [ForeignKey(nameof(Alternative))]
    [InverseProperty(nameof(Voice.UsedAsAlternativeIn))]
    public required Voice AlternativeVoiceNav { get; set; }
}