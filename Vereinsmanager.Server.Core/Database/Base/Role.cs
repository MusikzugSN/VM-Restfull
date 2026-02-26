using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.Base;

[Index(nameof(Name), IsUnique = true)]
public class Role : MetaData
{
    [Key]
    public int RoleId { get; set; }
    
    [Required]
    [MaxLength(24)]
    public required string Name { get; set; }
    
    public virtual ICollection<Permission> Permissions { get; private set; } = [];
    
    public MetaData NewestMetaData =>
        Permissions
            .Select(x => x as MetaData)
            .Append(this)
            .OrderByDescending(x => x.UpdatedAt)
            .First();

    public DateTime EffectiveLastChangedAt => NewestMetaData.UpdatedAt;

    public string EffectiveLastChangedBy => NewestMetaData.UpdatedBy;

}