#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.DataTransferObjects.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/role")]
public class RoleController : ControllerBase
{

    [HttpPost]
    public ActionResult<RoleDto> CreateRole(
        [FromBody] CreateRole createRole,
        [FromServices] RoleService roleService)
    {
        var newRole = roleService.CreateRole(createRole);

        if (newRole.IsSuccessful())
        {
            return new RoleDto(newRole.GetValue()!);
        }
        
        var problemDetails = newRole.GetProblemDetails();
        return StatusCode(problemDetails?.Status ?? 500, problemDetails?.Title ?? "Unkown error");
    }
}