#nullable enable
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
            
            return BadRequest(AuthFailedMessage + " username");
        }

        if (user.PasswordHash != loginRequest.Password)
        {
            return BadRequest(AuthFailedMessage + " password");
        }

        if (!user.IsEnabled)
        {
            return BadRequest(AuthFailedMessage + "  disabled");
        }
        
        var token = jwtTokenService.GenerateToken(user, 12.0);
        return new LoginResponse(token);
    }
    public record LoginResponse(string Token);
    public record LoginRequest(string Username, string Password);
}