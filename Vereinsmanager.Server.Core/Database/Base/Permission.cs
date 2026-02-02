#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.Base;

public class Permission : MetaData
{
    [Key]
    public int PermissionId { get; set; }
    
    [Required]
    public required Role Role { get; set; }
    
    public virtual int RoleId { get; private set; }
    
    [Required]
    public required int PermissionType { get; set; }
    
    [Required]
    public required int PermissionValue { get; set; }
}