using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.Base;

[Index(nameof(UserId), nameof(RoleId), nameof(GroupId), IsUnique = true)]
public class UserRole : MetaData
{
    [Key]
    public int UserRoleId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    [Required]
    public int RoleId { get; set; }
    [Required]
    public int GroupId { get; set; }
    
    
    public User? User { get; set; }
    public Role? Role { get; set; }
    public Group? Group { get; set; }
    
}