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
    
    [PermissionDescription(PermissionGroup.LoginSettings, PermissionCategory.Start)]
    OpenLoginSettings,
    [PermissionDescription(PermissionGroup.LoginSettings, PermissionCategory.Read)]
    ListLoginSettings,
    [PermissionDescription(PermissionGroup.LoginSettings, PermissionCategory.Create)]
    CreateLoginSettings,
    [PermissionDescription(PermissionGroup.LoginSettings, PermissionCategory.Update)]
    UpdateLoginSettings,
    [PermissionDescription(PermissionGroup.LoginSettings, PermissionCategory.Delete)]
    DeleteLoginSettings,
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