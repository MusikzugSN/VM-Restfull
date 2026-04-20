using System.IdentityModel.Tokens.Jwt;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.IO;
using Newtonsoft.Json.Serialization;
using Vereinsmanager.Autofac;
using Vereinsmanager.Database;
using Vereinsmanager.Services;
using Vereinsmanager.Utils;
using Vereinsmanager.Utils.Middleware;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    // Use the default property (Pascal) casing
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
});

var licenseKey = File.ReadAllText("syncfusion-license.txt");
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();


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
    var keyPath = (builder.Configuration["Jwt:KeyPath"] ?? "data/keys") + "/public_key.pem";
    var publicRsa = JwtTokenService.LoadPublicKey(keyPath); 
    
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
    //init loading keys to make them available
    await JwksLoader.LoadKeysAsync(provider.IssuerUrl);
    
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

var app = builder.Build();
app.MapHealthChecks("/health");
app.UseCors("DynamicCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<NoUserEditsValidatiorMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Update database to latest version

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServerDatabaseContext>();

    try
    {
        if (db.Database.CanConnect())
        {
            Log.Information("MySQL erfolgreich verbunden.");
        }
        else
        {
            Log.Fatal("MySQL nicht verfügbar. Server stoppt.");
            Environment.Exit(1);
        }
        
        var pendingMigations = db.Database.GetPendingMigrations().ToList();
        if (pendingMigations.Count > 0)
        {
            Log.Information("MySQL Migrationen ausstehend: {Migrations} Migrationen", pendingMigations.Count);
            db.Database.Migrate();
            Log.Information("MySQL erfolgreich aktuallisiert");    
        }
        
    }
    catch (MySqlException ex)
    {
        Log.Fatal(ex, "MySQL nicht verfügbar. Server stoppt.");
        Environment.Exit(1);
    }
    catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysql)
    {
        // Migration oder Update schlägt wegen MySQL-Fehler fehl
        Log.Fatal(mysql, "MySQL Migration fehlgeschlagen. Server stoppt.");
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        // Fallback für alles andere
        Log.Fatal(ex, "Unbekannter MySQL Fehler beim Start. Server stoppt.");
        Environment.Exit(1);
    }
}

app.Run();
