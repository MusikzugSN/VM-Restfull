using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(Name), IsUnique = true)]

public class Tag : MetaData
{
    [Required]
    public int TagId { get; set; }
    
    [Required]
    [MaxLength(24)]
    public required string Name { get; set; }
    
    public ICollection<TagUser>? TagUsers { get; set; }
}



