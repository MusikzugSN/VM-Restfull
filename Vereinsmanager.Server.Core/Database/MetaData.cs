#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vereinsmanager.Database;

public class MetaData
{
    [Required]
    public string CreatedBy { get; set; } = string.Empty;
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public DateTime CreatedAt { get; set; }

    [Required]
    public string UpdatedBy { get; set; } = string.Empty;
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }
}