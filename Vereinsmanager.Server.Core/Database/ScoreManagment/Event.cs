using System.ComponentModel.DataAnnotations;
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Database.ScoreManagment;

public class Event : MetaData
{
    [Key]
    public int EventId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }

    [Required]
    public List<EventScore> EventScore { get; set; } = new();
}