using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.ScoreManagment;

public class Score : MetaData
{
    [Key]
    public int ScoreId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Composer { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Link { get; set; } = string.Empty;
    
    [Required]
    public int Duration { get; set; }
}