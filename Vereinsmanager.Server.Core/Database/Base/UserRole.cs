using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.Base;

[Index(nameof(UserId), nameof(RoleId), nameof(GroupId), IsUnique = true)]
public class UserRole : MetaData
{
    [Key]
    public int UserRoleId { get; set; }
    
    [Required]
    public required User User { get; set; }
    public int UserId { get; private set; }
    
    [Required]
    public required Role Role { get; set; }
    public virtual int RoleId { get; private set; }
    
    [Required]
    public required Group Group { get; set; }
    public virtual int GroupId { get; private set; }
}