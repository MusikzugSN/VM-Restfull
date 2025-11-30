#nullable enable
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Vereinsmanager.Database.Base;

namespace Vereinsmanager.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, double hours)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()), 
            new Claim("user_id", user.UserId.ToString()), // Trik 17, da "sub" im HttpIntersceptor falsch aufgelößt wird
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim("last_edited", user.UpdatedAt.Ticks.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(hours),
            signingCredentials: creds); // todo far: public private keypair einbauen und mischicken?! 

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}