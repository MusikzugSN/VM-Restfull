#nullable enable
using Vereinsmanager.Database.Base;

namespace Vereinsmanager.DataTransferObjects.Base;

public class UserDto : MetaDataDto
{
    public int UserId { get; init; }
    public string Username { get; init; }

    public bool IsAdmin { get; init; }
    
    public bool IsEnabled { get; init; }
    
    public UserDto(User  user)
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