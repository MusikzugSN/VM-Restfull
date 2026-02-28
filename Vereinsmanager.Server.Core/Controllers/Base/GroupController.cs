#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Controllers;

[Authorize]
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

        return(ObjectResult)groups;
    }

    [HttpPatch]
    [Route("{groupId:int}")]
    public ActionResult<GroupDto> UpdateGroup(
        [FromRoute] int groupId,
        [FromBody] UpdateGroup updateGroup,
        [FromServices] GroupService groupService)
    {
        var updatedGroup = groupService.UpdateGroup(groupId, updateGroup);

        if (updatedGroup.IsSuccessful())
        {
            return new GroupDto(updatedGroup.GetValue()!);
        }

        return (ObjectResult)updatedGroup;
    }
    
    [HttpDelete]
    [Route("{deleteGroup:int}")]
    public ActionResult<bool> DeleteGroup(
        [FromRoute] int deleteGroup,
        [FromServices] GroupService groupService)
    {
        var deletedGroup = groupService.DeleteGroup(deleteGroup);

        if (deletedGroup.IsSuccessful())
        {
            return deletedGroup.GetValue();
        }

        return (ObjectResult)deletedGroup;
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

        return (ObjectResult)newGroup;
    }
}