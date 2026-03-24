using System.Security.Claims;
using Vereinsmanager.Database;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Utils;

public class UserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<ServerDatabaseContext> _dbContextLazy;
    private readonly IConfiguration _configuration;

    private User? UserModel { get; set; }

    public UserContext(IHttpContextAccessor httpContextAccessor, Lazy<ServerDatabaseContext> dbContextLazy, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContextLazy = dbContextLazy;
        _configuration = configuration;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    
    public string? UserId => User?.FindFirst("user_id")?.Value;
    
    public string? Subject => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? Issuer  => User?.FindFirst("iss")?.Value;

    public User? GetUserModel()
    {
        if (UserModel != null)
            return UserModel;

        if (!string.IsNullOrWhiteSpace(UserId))
        {
            var db = _dbContextLazy.Value;
            UserModel = db.Users.FirstOrDefault(u => u.UserId.ToString() == UserId);
            return UserModel;
        }
        
        if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Issuer))
            return null;

        var providers = _configuration
            .GetSection("OAuthProviders")
            .Get<OAuthConfig[]>() ?? [];
        var provider = providers.FirstOrDefault(p => p.IssuerUrl == Issuer);
        
        if (providers.Length == 0 || provider == null)
            return null;
        
        var dbProviders = _dbContextLazy.Value;
        UserModel = dbProviders.Users
            .FirstOrDefault(u =>
                u.OAuthSubject == Subject &&
                u.Provider == provider.ProviderKey);

        if (UserModel == null)
        {
            var loginConfig = LoadLoginConfig();
            if (!loginConfig.autoCreate)
                return null;
            
            var name =
                User?.FindFirst("preferred_username")?.Value ??
                User?.FindFirst("name")?.Value ??
                User?.FindFirst(ClaimTypes.Name)?.Value ??
                BuildNameFromClaims(User);
            
            var user = new User
            {
                Username = name ?? GenerateUserName(provider.ProviderKey, Subject),
                OAuthSubject = Subject,
                Provider = provider.ProviderKey,
                IsAdmin = false
            };
            dbProviders.Users.Add(user);
            
            if (loginConfig.defaultGroup.HasValue && loginConfig.defaultRole.HasValue)
            {
                var userGroup = new UserRole
                {
                    User = user,
                    RoleId = loginConfig.defaultRole.Value,
                    GroupId = loginConfig.defaultGroup.Value
                };
                dbProviders.UserRoles.Add(userGroup);
            }
            
            UserModel = user;
            dbProviders.SaveChanges();
        }
        
        return UserModel;
    }

    private string GenerateUserName(string provider, string userClaim)
    {
        var combinde = provider + '_' + userClaim;
        return combinde[..23];
    }
    
    private (bool autoCreate, int? defaultGroup, int? defaultRole) LoadLoginConfig()
    {
        var types = new[]
        {
            ConfigType.OAuthAutoCreateUsers,
            ConfigType.OAuthDefaultGroup,
            ConfigType.OAuthDefaultRole
        };

        var configs = _dbContextLazy.Value.Configurations
            .Where(c => types.Contains(c.Type))
            .ToDictionary(c => c.Type, c => c.Value);

        bool autoCreate =
            configs.TryGetValue(ConfigType.OAuthAutoCreateUsers, out var v1)
            && bool.TryParse(v1, out var b1)
            && b1;

        int? defaultGroup =
            configs.TryGetValue(ConfigType.OAuthDefaultGroup, out var v2)
            && int.TryParse(v2, out var g2)
                ? g2
                : null;

        int? defaultRole =
            configs.TryGetValue(ConfigType.OAuthDefaultRole, out var v3)
            && int.TryParse(v3, out var r3)
                ? r3
                : null;

        return (autoCreate, defaultGroup, defaultRole);
    }
    
    private static string? BuildNameFromClaims(ClaimsPrincipal? user)
    {
        var given = user?.FindFirst(ClaimTypes.GivenName)?.Value
                    ?? user?.FindFirst("given_name")?.Value;

        var family = user?.FindFirst(ClaimTypes.Surname)?.Value
                     ?? user?.FindFirst("family_name")?.Value;

        if (!string.IsNullOrWhiteSpace(given) && !string.IsNullOrWhiteSpace(family))
            return $"{given} {family}";

        return given ?? family;
    }
}