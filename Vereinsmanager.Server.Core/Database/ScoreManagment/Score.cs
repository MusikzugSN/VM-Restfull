using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(Title), IsUnique = true)]
public class Score : MetaData
{
    [Key]
    public int ScoreId { get; set; }

    [Required]
    [MaxLength(24)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Composer { get; set; }

    [MaxLength(255)]
    public string? Link { get; set; }

    public int? Duration { get; set; }
}