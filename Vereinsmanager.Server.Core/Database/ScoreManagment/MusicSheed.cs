using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(FileHash), nameof(FilePath), IsUnique = true)]
public class MusicSheed : MetaData
{
    [Key]
    public int MusicSheedId { get; set; }
    
    [Required] 
    [MaxLength(255)] 
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)] 
    public string FilePath { get; set; } = string.Empty;
    
    [Required] 
    [MaxLength(255)] 
    public string FileHash { get; set; } = string.Empty;
    
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