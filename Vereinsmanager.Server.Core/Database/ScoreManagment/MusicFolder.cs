using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database.Base;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(GroupId), nameof(Name), IsUnique = true)]
public class MusicFolder : MetaData
{
    [Key]
    public int MusicFolderId { get; set; }
    
    [Required]
    public required Group Group { get; set; }
    public int GroupId { get; set; }
    
    [Required]
    [MaxLength(24)]
    public required string Name { get; set; }
    
    public ICollection<ScoreMusicFolder> ScoreMusicFolders { get; set; } = new List<ScoreMusicFolder>();
}