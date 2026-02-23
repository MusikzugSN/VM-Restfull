#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;
using Vereinsmanager.Services.Models;

namespace Vereinsmanager.Controllers;

public record PermissionGroup(string? Name, List<PermissionValue> PermissionValues);
public record PermissionValue(PermissionType PermissionType, PermissionCategory PermissionCategory);

[Authorize]
[ApiController]
[Route("api/v1/role")]
public class RoleController : ControllerBase
{
    [HttpGet]
    [Route("permissionValues")]
    public ActionResult<List<PermissionGroup>> GetPermissionValues()
    {
        return Enum.GetValues<PermissionType>()
            .GroupBy(x => x.GetPermissionGroup())
            .Select(g =>
                new PermissionGroup(g.Key.GetDescription(),
                g.Select(x => 
                        new PermissionValue(x, x.GetPermissionCategory()))
                    .ToList()))
            .ToList();
    }

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
        
        return (ObjectResult) newRole;
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
        
        return (ObjectResult) updatedRole;
    }
    
    [HttpGet]
    public ActionResult<RoleDto[]> GetRoles([FromServices] RoleService roleService)
    {
        var roles = roleService.ListRoles();

        if (roles.IsSuccessful())
        {
            return roles.GetValue()!
                .Select(r => new RoleDto(r))
                .ToArray();
        }

        return (ObjectResult)roles;
    }

    [HttpDelete]
    [Route("{roleId:int}")]
    public ActionResult<bool> DeleteRole([FromRoute] int roleId, [FromServices] RoleService roleService)
    {
        var deletedRole = roleService.DeleteRole(roleId);

        if (deletedRole.IsSuccessful())
        {
            return deletedRole.GetValue();
        }
        
        return (ObjectResult)deletedRole;
    }
}