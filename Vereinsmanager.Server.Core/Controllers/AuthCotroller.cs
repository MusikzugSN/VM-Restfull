using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthCotroller : ControllerBase
{
    private static string AuthFailedMessage = "Authentication failed.";
    
    [HttpPost("login")]
    public ActionResult<LoginResponse> LoginViaUsernamePassword(
        [FromBody] LoginRequest loginRequest,
        [FromServices] JwtTokenService jwtTokenService,
        [FromServices] UserService userService)
    {
        var user = userService.LoadUserByUsername(loginRequest.Username);
        if (user == null)
        {
            if (userService.CountAdmins() == 0)
            {
                var installUser = userService.GetInstallUser();
                if (loginRequest.Username == installUser.Username && loginRequest.Password == installUser.PasswordHash) 
                    return new LoginResponse(jwtTokenService.GenerateToken(installUser, 0.2));
            }
            
            return BadRequest(AuthFailedMessage);
        }

        if (user.PasswordHash != loginRequest.Password)
        {
            return BadRequest(AuthFailedMessage);
        }

        if (!user.IsEnabled)
        {
            return BadRequest(AuthFailedMessage);
        }
        
        var token = jwtTokenService.GenerateToken(user, 12.0);
        return new LoginResponse(token);
    }
    public record LoginResponse(string Token);
    public record LoginRequest(string Username, string Password);
    
    [HttpGet("oAuthProvider")]
    public ActionResult<OAuthConfig[]> ListProvider(
        [FromServices] IConfiguration configuration)
    {
        return configuration
            .GetSection("OAuthProviders")
            .Get<OAuthConfig[]>() ?? [];
    }
    
    [HttpGet("me")]
    [Authorize]
    public ActionResult<MeDto> GetCurrentUser(
        [FromServices] UserContext userContext,
        [FromServices] IConfiguration configuration)
    {
        var user = userContext.GetUserModel();
        if (user == null)
            return Unauthorized();
        
        var providers = configuration
            .GetSection("OAuthProviders")
            .Get<OAuthConfig[]>() ?? [];
        var provider = providers.FirstOrDefault(p => p.ProviderKey == user.Provider);
        
        return new MeDto(user.UserId, user.Username, provider?.DisplayName ?? user.Provider, user.OAuthSubject);
    }
    
    public record MeDto(int UserId, string Username, string? Provider, string? OAuthSubject);
}