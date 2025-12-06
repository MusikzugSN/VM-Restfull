#nullable enable
namespace Vereinsmanager.Services.Models;

public enum PermissionType
{
    Administrator = 0,
    Open_Users = 1,
    Create_User,
    Update_User,
    
    Open_Groups = 20,
    Create_Group,
    Update_Group,
    Delete_Group,
    
    Open_Roles = 40,
    Create_Role,
    Update_Role,
    Delete_Role,
    //Read_User,
    //Delete_User,
    //Disable_User,
    //ResetPassword_User,
    
    
}