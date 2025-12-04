#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
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

    
    [HttpPatch]
    [Route("{id:int}")]
    public ActionResult<RoleDto> UpdateRole(
        [FromRoute] int id,
        [FromBody] UpdateRole updateRole,
        [FromServices] RoleService roleService)
    {
        var updatedRole = roleService.UpdateRole(id, updateRole);

        if (updatedRole.IsSuccessful())
        {
            return new RoleDto(updatedRole.GetValue()!);
        }
        
        var problemDetails = updatedRole.GetProblemDetails();
        return StatusCode(problemDetails?.Status ?? 500, problemDetails?.Title ?? "Unkown error");
    }
}