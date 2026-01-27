#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/group")]
public class GroupController : ControllerBase
{
    [HttpGet]
    public ActionResult<GroupDto[]> ListGroups(
        [FromServices] GroupService groupService)
    {
        var groups = groupService.ListGroups();

        if (groups.IsSuccessful())
        {
            return groups.GetValue()!.Select(g => new GroupDto(g)).ToArray();
        }
        
        var problemDetails = groups.GetProblemDetails();
        return StatusCode(problemDetails?.Status ?? 500, problemDetails?.Title ?? "Unknown error");
    }
    
    [HttpPost]
    public ActionResult<GroupDto> CreateGroup(
        [FromBody] CreateGroup createGroup,
        [FromServices] GroupService groupService)
    {
        var newGroup = groupService.CreateGroup(createGroup);

        if (newGroup.IsSuccessful())
        {
            return new GroupDto(newGroup.GetValue()!);
        }

        var problemDetails = newGroup.GetProblemDetails();
        return StatusCode(problemDetails?.Status ?? 500, problemDetails?.Title ?? "Unknown error");
    }
}