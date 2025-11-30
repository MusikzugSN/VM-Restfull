#nullable enable
using System.IdentityModel.Tokens.Jwt;
using Vereinsmanager.Database.Authentication;
using Vereinsmanager.Services;

namespace Vereinsmanager.Utils;

public class UserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<UserService> _userServiceLazy;
    
    private UserModel? _userModel;

    public UserContext(IHttpContextAccessor httpContextAccessor, Lazy<UserService> userServiceLazy)
    {
        _httpContextAccessor = httpContextAccessor;
        _userServiceLazy = userServiceLazy;
    }
    
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public UserModel? GetUserModel()
    {
        if (UserId == null)
            return null;
        
        _userModel ??= _userServiceLazy.Value.LoadUserById(int.Parse(UserId));
        
        return _userModel;
    }
}