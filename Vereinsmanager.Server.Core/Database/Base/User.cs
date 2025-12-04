#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.Base;

public class User : MetaData
{
    [Key]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(24)]
    public required string Username { get; set; }
    [Required]
    [MaxLength(255)]
    public required string PasswordHash { get; set; }

    [Required] 
    public bool IsAdmin { get; set; }
    
    [Required]
    public bool IsEnabled { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}