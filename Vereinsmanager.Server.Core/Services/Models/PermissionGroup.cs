#nullable enable
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.Models;

public enum PermissionGroup
{
    [Description("Administrator (übergreifend)")]
    Administrator = 0,
    [Description("Benutzerverwaltung (übergreifend)")]
    UserManagement = 1,
    [Description("Gruppenverwaltung (übergreifend)")]
    GroupManagement = 2,
    [Description("Rollenverwaltung (übergreifend)")]
    RoleManagement = 3,
    [Description("Logineinstellungen (übergreifend)")]
    LoginSettings = 4,
    [Description("Stimmenverwaltung (übergreifend)")]
    VoiceManagement = 5,
    [Description("Notenverwaltung (übergreifend)")]
    ScoreManagement = 6,
    [Description("Eventverwaltung (übergreifend)")]
    EventManagement = 7,
    [Description("Mappenverwaltung (übergreifend)")]
    MusicSheetManagement = 8,
    [Description("Instrumentenverwaltung (übergreifend)")]
    InstrumentManagement = 10,
    
}

public static class PermissionGroupHelper
{
    public static string? GetDescription(this PermissionGroup value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field != null)
        {
            if (Attribute.GetCustomAttribute(field, typeof(Description)) is Description attribute)
            {
                return attribute.Text;
            }
        }
        return null;
    }
}