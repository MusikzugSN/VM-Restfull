using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(VoiceId), nameof(AlternativeId), IsUnique = true)]
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
    public int AlternativeId { get; set; }

    [ForeignKey(nameof(AlternativeId))]
    [InverseProperty(nameof(Voice.UsedAsAlternativeIn))]
    public required Voice AlternativeVoiceNav { get; set; }
}