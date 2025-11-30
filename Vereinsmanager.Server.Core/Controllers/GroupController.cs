#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.DataTransferObjects.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Controllers;

[ApiController]
[Route("api/v1/group")]
public class GroupController : ControllerBase
{

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