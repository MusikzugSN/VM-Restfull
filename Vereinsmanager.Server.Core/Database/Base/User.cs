#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.Base;

[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Provider), nameof(OAuthSubject), IsUnique = true)]
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
    
    [MaxLength(64)]
    public string? Provider { get; set; }
    
    [MaxLength(255)]
    public string? OAuthSubject { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    
    public MetaData NewestMetaData =>
        UserRoles
            .Select(x => x as MetaData)
            .Append(this)
            .OrderByDescending(x => x.UpdatedAt)
            .First();

    public DateTime EffectiveLastChangedAt => NewestMetaData.UpdatedAt;

    public string EffectiveLastChangedBy => NewestMetaData.UpdatedBy;
}