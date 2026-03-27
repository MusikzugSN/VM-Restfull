using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereinsmanager.Database;
using Vereinsmanager.Services;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Controllers.Base;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private const string AuthFailedMessage = "login_failed";

    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<LoginResponse> LoginViaUsernamePassword(
        [FromBody] LoginRequest loginRequest,
        [FromServices] JwtTokenService jwtTokenService,
        [FromServices] UserService userService)
    {
        if (userService.IsPasswordLoginDisabled())
        {
            return BadRequest("Password login is disabled");
        }
        
        var user = userService.LoadUserByUsername(loginRequest.Username);
        if (user == null)
        {
            if (userService.CountAdmins() == 0)
            {
                var installUser = userService.GetInstallUser();
                if (loginRequest.Username == installUser.Username && loginRequest.Password == installUser.PasswordHash) 
                    return new LoginResponse(jwtTokenService.GenerateToken(installUser, 0.2));
            }
            
            return Unauthorized(AuthFailedMessage);
        }

        if (!userService.VerifyUserPassword(loginRequest.Password, user.PasswordHash ?? ""))
        {
            return Unauthorized(AuthFailedMessage);
        }

        if (!user.IsEnabled)
        {
            return StatusCode(403, AuthFailedMessage);
        }
        
        if (user.OAuthSubject != null && user.Provider != null && !userService.IsLocalLoginForOAuthUsersAllowed())
        {
            return StatusCode(403, AuthFailedMessage);
        }
        
        var token = jwtTokenService.GenerateToken(user, 12.0);
        return new LoginResponse(token);
    }
    public record LoginResponse(string Token);
    public record LoginRequest(string Username, string Password);
    
    [AllowAnonymous]
    [HttpGet("oAuthProvider")]
    public ActionResult<OAuthConfig[]> ListProvider(
        [FromServices] IConfiguration configuration)
    {
        return configuration
            .GetSection("OAuthProviders")
            .Get<OAuthConfig[]>() ?? [];
    }
    
    [AllowAnonymous]
    [HttpGet("print/verifyToken")]
    public ActionResult<TokenData> VerifyToken([FromQuery] string token, 
        [FromServices] CustomTokenService customTokenService)
    {
        var data = customTokenService.ValidateToken(token, "print");
        if (data == null)
            return BadRequest();
        
        return data;
    }
    
    [Authorize]
    [HttpGet("print/generateToken")]
    public ActionResult<string> GenerateToken(
        [FromServices] UserContext userContext,
        [FromServices] CustomTokenService customTokenService)
    {
        var data = customTokenService.GenerateToken("print", userContext.GetUserModel()?.UserId ?? -1);
        return data;
    }
    
    [Authorize]
    [HttpGet("me")]
    public ActionResult<MeDto> GetCurrentUser(
        [FromServices] UserContext userContext,
        [FromServices] IConfiguration configuration,
        [FromServices] Lazy<ServerDatabaseContext> dbContextLazy)
    {
        var userModel = userContext.GetUserModel();
        if (userModel == null)
            return new MeDto(-2, "Unauthorized", null, null, false, []);
        
        var providers = configuration
            .GetSection("OAuthProviders")
            .Get<OAuthConfig[]>() ?? [];
        var provider = providers.FirstOrDefault(p => p.ProviderKey == userModel.Provider);
        
        var permissions = new List<PermissionTeaserWithGroup>();
        if (userModel.IsAdmin)
            return new MeDto(userModel.UserId, userModel.Username, provider?.DisplayName ?? userModel.Provider,
                userModel.OAuthSubject, userModel.IsAdmin, permissions.ToArray());
        
        var userFromDb = dbContextLazy.Value.Users
            .Where(u => u.UserId == userModel.UserId)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role!)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefault();
            
        foreach (var userUserRole in userFromDb?.UserRoles ?? [])
        {
            var groupId = userUserRole.GroupId;
            foreach (var rolePermission in userUserRole.Role?.Permissions ?? [])
            {
                permissions.Add(new PermissionTeaserWithGroup(groupId, rolePermission.PermissionType, rolePermission.PermissionValue));
            }
        }
        return new MeDto(userModel.UserId, userModel.Username, provider?.DisplayName ?? userModel.Provider, userModel.OAuthSubject, userModel.IsAdmin, permissions.ToArray());
    }
    
    public record MeDto(int UserId, string Username, string? Provider, string? OAuthSubject, bool IsAdmin, PermissionTeaserWithGroup[] Permissions);

    public record PermissionTeaserWithGroup(int GroupId, int PermissionType, int PermissionValue);
}