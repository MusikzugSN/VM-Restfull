using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(Alternative), nameof(VoiceId), nameof(Priority), IsUnique = true)]
public class AlternateVoice
{ 
    [Key]
    public int AlternateVoiceId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Alternative { get; set; } = string.Empty;
    
    [Required]
    public int Priority { get; set; }

    [Required]
    public required Voice Voice { get; set; } 
    public int VoiceId { get; set; }
}