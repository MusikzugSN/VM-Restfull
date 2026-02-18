using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(ScoreId), nameof(MusicFolderId), IsUnique = true)]

public class ScoreMusicFolder : MetaData
{
    [Key]
    public int ScoreMusicFolderId { get; set; } 

    [Required]
    public required Score Score { get; set; }
    public int ScoreId { get; set; }

    [Required]
    public required MusicFolder MusicFolder { get; set; }
    public int MusicFolderId { get; set; }
    
    [Required]
    public int Number { get; set; }
}