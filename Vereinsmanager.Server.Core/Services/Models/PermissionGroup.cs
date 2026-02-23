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
    [Description("Stimmenverwaltung")]
    VoiceManagement = 4,
    [Description("Notenverwaltung")]
    ScoreManagement = 5,
    [Description("Eventverwaltung")]
    EventManagement = 6,
    [Description("Musikmappenverwaltung")]
    MusicFolderManagement = 7,
    [Description("Musikblattverwaltung")]
    MusicSheetManagement = 8,
    [Description("Instrumentenverwaltung")]
    InstrumentManagement = 9,
    
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