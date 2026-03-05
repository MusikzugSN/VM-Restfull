using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vereinsmanager.Database.ScoreManagment;

public class Voice : MetaData
{
    [Key]
    public int VoiceId { get; set; }

    [Required]
    [MaxLength(24)]
    public required string Name { get; set; }

    [Required]
    public required Instrument Instrument { get; set; }
    public int InstrumentId { get; set; }

    public List<MusicSheet>? MusicSheets { get; set; }

    [InverseProperty(nameof(AlternateVoice.Voice))]
    public List<AlternateVoice>? AlternateVoices { get; set; }

    [InverseProperty(nameof(AlternateVoice.AlternativeVoiceNav))]
    public List<AlternateVoice>? UsedAsAlternativeIn { get; set; }
}