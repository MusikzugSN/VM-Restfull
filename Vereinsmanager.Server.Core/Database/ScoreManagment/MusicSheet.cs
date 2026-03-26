using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

public enum MusicSheetStatus
{
    Ungeprueft = 0,
    Geprueft = 1
}

[Index(nameof(ScoreId), nameof(VoiceId), IsUnique = true)]
public class MusicSheet : MetaData
{
    [Key]
    public int MusicSheetId { get; set; }

    [Required]
    public int Filesize { get; set; }

    [Required]
    public int PageCount { get; set; }

    [Required]
    [MaxLength(255)]
    public required string FileName { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string FileHash { get; set; }

    [Required]
    public required int ScoreId { get; set; }    
    
    [Required]
    public required int VoiceId { get; set; }
    
    [Required]
    public MusicSheetStatus Status { get; set; } = MusicSheetStatus.Ungeprueft;

    public ICollection<TagUser>? TagUsers { get; set; } 
    
    public Score? Score { get; set; }
    public Voice? Voice { get; set; }
    
}