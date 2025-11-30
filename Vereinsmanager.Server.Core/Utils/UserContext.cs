#nullable enable
using System.IdentityModel.Tokens.Jwt;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Utils;

public class UserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<UserService> _userServiceLazy;
    
    private User? _userModel;

    public UserContext(IHttpContextAccessor httpContextAccessor, Lazy<UserService> userServiceLazy)
    {
        _httpContextAccessor = httpContextAccessor;
        _userServiceLazy = userServiceLazy;
    }
    
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

    public User? GetUserModel()
    {
        if (UserId == null)
            return null;
        
        _userModel ??= _userServiceLazy.Value.LoadUserById(int.Parse(UserId));
        
        return _userModel;
    }
}