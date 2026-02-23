using Vereinsmanager.Database.Base;
using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.ScoreManagment;

public class MusicFolder: MetaData
{
    [Key]
    public int MusicFolderId { get; set; }
    
    [Required]
    public required Group Group { get; set; }
    public int GroupId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }
}
