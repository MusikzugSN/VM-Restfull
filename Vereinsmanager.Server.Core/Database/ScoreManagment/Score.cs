using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(Title), IsUnique = true)]
public class Score : MetaData
{
    [Key]
    public int ScoreId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Composer { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Link { get; set; }
    
    [Required]
    public int Duration { get; set; }
}