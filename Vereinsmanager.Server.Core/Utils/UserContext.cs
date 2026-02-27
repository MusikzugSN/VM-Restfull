using System.Security.Claims;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Utils;

public class UserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<ServerDatabaseContext> _dbContextLazy;
    private readonly IConfiguration _configuration;

    public User? UserModel { get; private set; }

    public UserContext(IHttpContextAccessor httpContextAccessor, Lazy<ServerDatabaseContext> dbContextLazy, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContextLazy = dbContextLazy;
        _configuration = configuration;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirst("user_id")?.Value;
    
    public string? Subject => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? Issuer  => _httpContextAccessor.HttpContext?.User.FindFirst("iss")?.Value;

    public User? GetUserModel()
    {
        if (UserModel != null)
            return UserModel;

        if (UserId != null)
        {
            var db = _dbContextLazy.Value;
            UserModel = db.Users.FirstOrDefault(u => u.UserId.ToString() == UserId);
            return UserModel;
        }
        
        if (Subject == null || Issuer == null)
            return null;

        var providers = _configuration.GetSection("OAuthProviders").Get<OAuthConfig[]>() ?? [];

        var provider = providers.FirstOrDefault(p => p.IssuerUrl == Issuer);
        
        if (providers.Length == 0 || provider == null)
            return null;
        
        var dbProviders = _dbContextLazy.Value;
        UserModel = dbProviders.Users
            .FirstOrDefault(u =>
                u.OAuthSubject == Subject &&
                u.Provider == provider.ProviderKey);

        return UserModel;
    }
}