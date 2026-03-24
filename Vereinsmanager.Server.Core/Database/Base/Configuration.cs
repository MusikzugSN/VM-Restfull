using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.Base;

public enum ConfigType
{
    // OAuth User Handling
    OAuthAutoCreateUsers = 0,
    OAuthDefaultGroup = 1,
    OAuthDefaultRole = 2,
    OAuthAllowPasswordLogin = 3,
    DisablePasswordLogin = 4,

    // UI / Navigation
    NavigationBarText = 10,

    // Impressum
    UseCustomImprint = 20,
    CustomImprintLink = 21
}
[Index(nameof(Type), IsUnique = true)]
public class Configuration : MetaData
{
    [Key]
    public ConfigType Type { get; set; }

    [Required]
    public string Value { get; set; }
}