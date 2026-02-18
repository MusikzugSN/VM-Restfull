using System.ComponentModel.DataAnnotations;
namespace Vereinsmanager.Database.ScoreManagment;
using Microsoft.EntityFrameworkCore;

[Index(nameof(EventId), nameof(ScoreId), IsUnique = true)]
public class EventScore: MetaData
{
    [Key]
    public int EventScoreId { get; set; }
    
    [Required]
    public required Event Event { get; set; } 
    public int EventId { get; set; }
    
    [Required]
    public required Score Score { get; set; } 
    public int ScoreId { get; set; }
}
