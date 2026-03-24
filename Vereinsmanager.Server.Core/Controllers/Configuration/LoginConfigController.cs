#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services;
using Vereinsmanager.Services.Models;

namespace Vereinsmanager.Controllers.Configuration;

[ApiController]
[Route("api/v1/config/login")]
public class LoginConfigController : ControllerBase
{
    private readonly ConfigurationService _configService;
    private readonly PermissionService _permissionService;

    public LoginConfigController(ConfigurationService configService, PermissionService permissionService)
    {
        _configService = configService;
        _permissionService = permissionService;
    }

    [HttpGet]
    public ActionResult<LoginConfigDto> GetLoginConfig()
    {
        var dto = new LoginConfigDto
        {
            OAuthAutoCreateUsers = GetBool(ConfigType.OAuthAutoCreateUsers),
            OAuthDefaultGroup = GetInt(ConfigType.OAuthDefaultGroup),
            OAuthDefaultRole = GetInt(ConfigType.OAuthDefaultRole),
            OAuthAllowPasswordLogin = GetBool(ConfigType.OAuthAllowPasswordLogin),
            DisablePasswordLogin = GetBool(ConfigType.DisablePasswordLogin),
            NavigationBarText = GetString(ConfigType.NavigationBarText),
            UseCustomImprint = GetBool(ConfigType.UseCustomImprint),
            CustomImpressumLink = GetString(ConfigType.CustomImprintLink)
        };

        return dto;
    }

    [Authorize]
    [HttpPost]
    public IActionResult UpdateLoginConfig([FromBody] LoginConfigDto dto)
    {
        if (!_permissionService.HasPermission(PermissionType.UpdateLoginSettings))
            return Forbid();

        SetBool(ConfigType.OAuthAutoCreateUsers, dto.OAuthAutoCreateUsers);
        SetInt(ConfigType.OAuthDefaultGroup, dto.OAuthDefaultGroup);
        SetInt(ConfigType.OAuthDefaultRole, dto.OAuthDefaultRole);
        SetBool(ConfigType.OAuthAllowPasswordLogin, dto.OAuthAllowPasswordLogin);
        SetBool(ConfigType.DisablePasswordLogin, dto.DisablePasswordLogin);
        SetString(ConfigType.NavigationBarText, dto.NavigationBarText);
        SetBool(ConfigType.UseCustomImprint, dto.UseCustomImprint);
        SetString(ConfigType.CustomImprintLink, dto.CustomImpressumLink);

        return Ok();
    }

    // -------------------------
    // Helpers
    // -------------------------

    private string? GetString(ConfigType type)
        => _configService.GetConfiguration(type)?.Value;

    private bool GetBool(ConfigType type)
        => bool.TryParse(_configService.GetConfiguration(type)?.Value, out var v) && v;

    private int? GetInt(ConfigType type)
        => int.TryParse(_configService.GetConfiguration(type)?.Value, out var v) ? v : null;

    private void SetString(ConfigType type, string? value)
    {
        if (value != null)
            _configService.CreateOrUpdateConfiguration(new CreateConfiguration(type, value));
    }

    private void SetBool(ConfigType type, bool? value)
    {
        if (value != null)
            _configService.CreateOrUpdateConfiguration(new CreateConfiguration(type, value.ToString()));
    }

    private void SetInt(ConfigType type, int? value)
    {
        if (value != null)
            _configService.CreateOrUpdateConfiguration(new CreateConfiguration(type, value.ToString()));
    }
}

public class LoginConfigDto
{
    public bool? OAuthAutoCreateUsers { get; set; }
    public int? OAuthDefaultGroup { get; set; }
    public int? OAuthDefaultRole { get; set; }
    public bool? OAuthAllowPasswordLogin { get; set; }
    public bool? DisablePasswordLogin { get; set; }
    public string? NavigationBarText { get; set; }
    public bool? UseCustomImprint { get; set; }
    public string? CustomImpressumLink { get; set; }
}