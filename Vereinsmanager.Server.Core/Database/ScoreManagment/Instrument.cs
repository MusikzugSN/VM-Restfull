using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.ScoreManagment;
public class Instrument:MetaData
{
    [Required]
    public int InstrumentId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public List<Voice> Voices { get; set; } = new();
}