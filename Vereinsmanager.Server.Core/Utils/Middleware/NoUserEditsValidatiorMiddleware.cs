#nullable enable
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Vereinsmanager.Utils.Middleware;

public class NoUserEditsValidatiorMiddleware
{
    private readonly RequestDelegate _next;
    
    public NoUserEditsValidatiorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, UserContext userContext)
    {
        var userId = httpContext.User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
        if (userId == (-1).ToString())
        {
            await _next(httpContext);
            return;
        }
        
        var tokenUserEditTimeStamp = Convert.ToInt64(httpContext.User.FindFirst(JwtRegisteredClaimNames.Iat)?.Value);
        var userModeTimeStamp = userContext.GetUserModel()?.UpdatedAt.Ticks;
        if (tokenUserEditTimeStamp > userModeTimeStamp)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        
        await _next(httpContext);
    }
}