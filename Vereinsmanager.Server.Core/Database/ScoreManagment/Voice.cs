using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(InstrumentId), nameof(Name), IsUnique = true)]
public class Voice : MetaData
{
    [Key]
    public int VoiceId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    
    [Required]
    public required Instrument Instrument { get; set; } 
    public int InstrumentId { get; set; }
    
    [Required]
    public List<MusicSheet> MusicSheets { get; set; } = new();
    
    [Required]
    public List<AlternateVoice> AlternateVoices { get; set; } = new();
}