#nullable enable
using Vereinsmanager.Database.Authentication;

namespace Vereinsmanager.DataTransferObjects.Authentication;

public class UserDto : MetaDataDTO
{
    public int UserId { get; set; }
    public string Username { get; set; }

    public bool IsAdmin { get; set; }
    
    public bool IsEnabled { get; set; }
    
    public UserDto(UserModel  user)
    {
        UserId = user.UserId;
        Username = user.Username;
        IsAdmin = user.IsAdmin;
        IsEnabled = user.IsEnabled;
        
        CreatedAt = user.CreatedAt;
        CreatedBy = user.CreatedBy;
        UpdatedAt = user.UpdatedAt;
        UpdatedBy = user.UpdatedBy;
    }
    
}