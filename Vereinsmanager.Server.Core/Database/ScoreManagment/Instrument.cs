using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.ScoreManagment;
public class Instrument:MetaData
{
    [Required]
    public int InstrumentId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Type { get; set; }
    
    [Required]
    public List<Voice> Voices { get; set; } = new();
}