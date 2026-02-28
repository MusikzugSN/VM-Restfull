using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(FileHash), nameof(FilePath), IsUnique = true)]
public class MusicSheet : MetaData
{
    [Key]
    public int MusicSheetId { get; set; }
    
    [Required]
    [MaxLength(255)] 
    public  required string FilePath { get; set; } 
    
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
    public required Score Score { get; set; }
    public virtual int ScoreId { get; set; }
    
    [Required]
    public required Voice  Voice { get; set; }
    public virtual int VoiceId {get; set;}
}