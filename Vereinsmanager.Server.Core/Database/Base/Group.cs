using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.Base;

[Index(nameof(Name), IsUnique = true)]
public class Group : MetaData
{
    [Key]
    public int GroupId { get; set; }
    
    [Required]
    [MaxLength(24)]
    public required string Name { get; set; }
}