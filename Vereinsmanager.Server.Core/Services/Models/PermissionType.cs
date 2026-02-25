#nullable enable
namespace Vereinsmanager.Services.Models;

public enum PermissionType
{
    // Start, Lesen, Schreiben, Löschen
    [PermissionDescription(PermissionGroup.Administrator, PermissionCategory.Start)]
    Administrator = 0,
    [PermissionDescription(PermissionGroup.UserManagement, PermissionCategory.Start)]
    OpenUser,
    [PermissionDescription(PermissionGroup.UserManagement, PermissionCategory.Read)]
    ListUser,
    [PermissionDescription(PermissionGroup.UserManagement, PermissionCategory.Create)]
    CreateUser,
    [PermissionDescription(PermissionGroup.UserManagement, PermissionCategory.Update)]
    UpdateUser,
    [PermissionDescription(PermissionGroup.UserManagement, PermissionCategory.Delete)]
    DeleteUser,
    
    [PermissionDescription(PermissionGroup.GroupManagement, PermissionCategory.Start)]
    OpenGroup,
    [PermissionDescription(PermissionGroup.GroupManagement, PermissionCategory.Read)]
    ListGroup,
    [PermissionDescription(PermissionGroup.GroupManagement, PermissionCategory.Create)]
    CreateGroup,
    [PermissionDescription(PermissionGroup.GroupManagement, PermissionCategory.Update)]
    UpdateGroup,
    [PermissionDescription(PermissionGroup.GroupManagement, PermissionCategory.Delete)]
    DeleteGroup,
    
    [PermissionDescription(PermissionGroup.RoleManagement, PermissionCategory.Start)]
    OpenRole,
    [PermissionDescription(PermissionGroup.RoleManagement, PermissionCategory.Read)]
    ListRole,
    [PermissionDescription(PermissionGroup.RoleManagement, PermissionCategory.Create)]
    CreateRole,
    [PermissionDescription(PermissionGroup.RoleManagement, PermissionCategory.Update)]
    UpdateRole,
    [PermissionDescription(PermissionGroup.RoleManagement, PermissionCategory.Delete)]
    DeleteRole,
    
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Start)]
    OpenVoice,
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Read)]
    ListVoice,
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Create)]
    CreateVoice,
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Update)]
    UpdateVoice,
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Delete)]
    DeleteVoice,
    
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Start)]
    CreateAlternateVoice,
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Read)]
    UpdateAlternateVoice,
    [PermissionDescription(PermissionGroup.VoiceManagement, PermissionCategory.Delete)]
    DeleteAlternateVoice,
    
    [PermissionDescription(PermissionGroup.ScoreManagement, PermissionCategory.Start)]
    OpenScores,
    [PermissionDescription(PermissionGroup.ScoreManagement, PermissionCategory.Read)]
    ListScore,
    [PermissionDescription(PermissionGroup.ScoreManagement, PermissionCategory.Create)]
    CreateScore,
    [PermissionDescription(PermissionGroup.ScoreManagement, PermissionCategory.Update)]
    UpdateScore,
    [PermissionDescription(PermissionGroup.ScoreManagement, PermissionCategory.Delete)]
    DeleteScore,
    
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Start)]
    OpenEvent,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Read)]
    ListEvent,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Create)]
    CreateEvent,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Update)]
    UpdateEvent,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Delete)]
    DeleteEvent,
    
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Start)]
    OpenEventScore,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Read)]
    ListEventScore,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Create)]
    CreateEventScore,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Update)]
    UpdateEventScore,
    [PermissionDescription(PermissionGroup.EventManagement, PermissionCategory.Delete)]
    DeleteEventScore,
    
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Start)]
    OpenMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Read)]
    ListMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Create)]
    CreateMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Update)]
    UpdateMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Delete)]
    DeleteMusicFolder,
    
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Start)]
    OpenScoreMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Read)]
    ListScoreMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Create)]
    CreateScoreMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Update)]
    UpdateScoreMusicFolder,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Delete)]
    DeleteScoreMusicFolder,
   
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Start)]
    OpenMusicSheet,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Read)]
    ListMusicSheet,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Create)]
    CreateMusicSheet,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Update)]
    UpdateMusicSheet,
    [PermissionDescription(PermissionGroup.MusicSheetManagement, PermissionCategory.Delete)]
    DeleteMusicSheet,
    
    [PermissionDescription(PermissionGroup.InstrumentManagement, PermissionCategory.Start)]
    OpenInstrument,
    [PermissionDescription(PermissionGroup.InstrumentManagement, PermissionCategory.Read)]
    ListInstrument,
    [PermissionDescription(PermissionGroup.InstrumentManagement, PermissionCategory.Create)]
    CreateInstrument,
    [PermissionDescription(PermissionGroup.InstrumentManagement, PermissionCategory.Update)]
    UpdateInstrument,
    [PermissionDescription(PermissionGroup.InstrumentManagement, PermissionCategory.Delete)]
    DeleteInstrument,

    
}

public static class PermissionTypeHelper
{
    public static PermissionGroup GetPermissionGroup(this PermissionType value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field != null)
        {
            if (Attribute.GetCustomAttribute(field, typeof(PermissionDescription)) is PermissionDescription attribute)
            {
                return attribute.Group;
            }
        }
        throw new InvalidOperationException("PermissionDescription attribute not found.");
    }
    
    public static PermissionCategory GetPermissionCategory(this PermissionType value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field != null)
        {
            if (Attribute.GetCustomAttribute(field, typeof(PermissionDescription)) is PermissionDescription attribute)
            {
                return attribute.Type;
            }
        }
        throw new InvalidOperationException("PermissionDescription attribute not found.");
    }
}