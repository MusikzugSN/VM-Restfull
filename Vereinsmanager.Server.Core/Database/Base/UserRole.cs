#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.Base;

public class UserRole : MetaData
{
    [Key]
    public int UserRoleId { get; set; }
    
    [Required]
    public required User User { get; set; }
    
    [Required]
    public required Role Role { get; set; }
    
    public virtual int RoleId { get; private set; }
    
    [Required]
    public required Group Group { get; set; }
    
    public virtual int GroupId { get; private set; }
}