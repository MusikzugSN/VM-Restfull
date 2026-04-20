#nullable enable
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        var path = (_config["Jwt:KeyPath"] ?? "data/keys/") + "private_key.pem";
        Console.WriteLine("JWT Key Path: " + _config["Jwt:KeyPath"]);
        Console.WriteLine($"Loading private key from: {path}");
        var privateRsa = LoadPrivateKey(path); 
        var key = new RsaSecurityKey(privateRsa); 
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(hours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public static RSA LoadPrivateKey(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Private key file not found: {path}");

        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(path));
        return rsa;
    }

    public static RsaSecurityKey LoadPublicKey(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Public key file not found: {path}");

        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(path));
        return new RsaSecurityKey(rsa);
    }

}