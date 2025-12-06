#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.Base;

public class Role : MetaData
{
    [Key]
    public int RoleId { get; set; }
    
    [Required]
    [MaxLength(24)]
    public string Name { get; set; }
    
    public virtual ICollection<Permission> Permissions { get; private set; } = [];
}