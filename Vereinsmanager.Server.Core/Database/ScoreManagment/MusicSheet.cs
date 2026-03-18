using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

public enum MusicSheetStatus
{
    Ungeprueft = 0,
    Geprueft = 1
}

[Index(nameof(ScoreId), nameof(VoiceId), IsUnique = true)]
[Index(nameof(FilePath), IsUnique = true)]
public class MusicSheet : MetaData
{
    [Key]
    public int MusicSheetId { get; set; }
    
    [Required]
    [MaxLength(255)] 
    public required string FilePath { get; set; } 
    
    [Required] 
    [MaxLength(255)] 
    public required string FileHash { get; set; }
    
    [Required]
    public int Filesize { get; set; }
    
    [Required]
    public int PageCount { get; set; }
    
    [Required]
    public DateTime FileModifiedDate { get; set; }

    [Required]
    public MusicSheetStatus Status { get; set; } = MusicSheetStatus.Ungeprueft;
    
    [Required]
    public required Score Score { get; set; }
    public virtual int ScoreId { get; set; }
    
    [Required]
    public required Voice Voice { get; set; }
    public virtual int VoiceId { get; set; }
}