#nullable enable
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Utils;

public class UserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<ServerDatabaseContext> _dbContextLazy;
    
    private User? _userModel;

    public UserContext(IHttpContextAccessor httpContextAccessor, Lazy<ServerDatabaseContext> dbContextLazy)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContextLazy = dbContextLazy;
    }
    
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

    public User? GetUserModel()
    {
        if (UserId == null)
            return null;
        
        _userModel ??= _dbContextLazy.Value.Users.FirstOrDefault(x => x.UserId == int.Parse(UserId));
        
        return _userModel;
    }
}