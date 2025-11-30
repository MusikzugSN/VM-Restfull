#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.Base;


public class Group : MetaData
{
    [Key]
    public int GroupId { get; set; }
    
    [Required]
    [MaxLength(64)]
    public required string Name { get; set; }
}