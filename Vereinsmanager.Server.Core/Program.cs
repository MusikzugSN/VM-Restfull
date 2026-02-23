using System.IdentityModel.Tokens.Jwt;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vereinsmanager.Autofac;
using Vereinsmanager.Database;
using Vereinsmanager.Services;
using Vereinsmanager.Utils;
using Vereinsmanager.Utils.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

var allowedOrigin = builder.Configuration["FRONTEND_URL"];
builder.Services.AddCors(options => {
    options.AddPolicy("DynamicCors", policy => {
        if (!string.IsNullOrWhiteSpace(allowedOrigin))
        {
            policy.WithOrigins(allowedOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        } else { 
            // fallback for dev if env var is missing
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        } 
    }); 
});

// Configure autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new SeverModule(builder.Configuration));
});

// Configure JWT Authentication
var authService = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "DynamicJwt";
    })
    .AddPolicyScheme("DynamicJwt", "Dynamic JWT", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
                return JwtBearerDefaults.AuthenticationScheme;

            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityToken(token);

            var issuer = jwt.Issuer;

            // Match issuer to provider
            var providers = builder.Configuration
                .GetSection("OAuthProviders")
                .Get<OAuthConfig[]>() ?? [];

            var provider = providers.FirstOrDefault(p => p.IssuerUrl == issuer);
            return provider?.ProviderKey ?? JwtBearerDefaults.AuthenticationScheme;
        };
    });

    
authService.AddJwtBearer(options =>
{
    var publicRsa = JwtTokenService.LoadPublicKey(builder.Configuration["Jwt:PublicKeyPath"] ?? "keys/public_key.pem"); 
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = publicRsa
    };
});

var providerConfigs = builder.Configuration
    .GetSection("OAuthProviders")
    .Get<OAuthConfig[]>() ?? [];

foreach (var provider in providerConfigs)
{
    authService.AddJwtBearer(provider.ProviderKey, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = provider.IssuerUrl,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var keys = JwksLoader.LoadKeysAsync(provider.IssuerUrl).Result; return keys;
            }
        };
    });
}


builder.Services.AddControllers();
var app = builder.Build();

app.UseCors("DynamicCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<NoUserEditsValidatiorMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Update database to latest version
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ServerDatabaseContext>();
db.Database.Migrate();

app.Run();