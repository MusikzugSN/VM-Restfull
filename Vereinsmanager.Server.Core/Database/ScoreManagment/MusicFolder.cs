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
    [MaxLength(255)]
    public required string Name { get; set; }
}