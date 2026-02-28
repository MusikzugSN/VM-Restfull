#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;

namespace Vereinsmanager.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/user")]
public class UserController : ControllerBase
{

    [HttpPost]
    public ActionResult<UserDto> CreateUser([FromBody] UserCreate userCreate, [FromServices] UserService userService)
    {
        //todo florian: berechtigungen prüfen
        var newUser = userService.CreateUser(userCreate);

        if (newUser.IsSuccessful())
        {
            return new UserDto(newUser.GetValue()!);
        }

        return (ObjectResult) newUser;
    }

    [HttpGet]
    public ActionResult<UserDto[]> GetAllUsers(
        [FromServices] UserService userService)
    {
        var allUsers = userService.ListUsers();
        
        if (allUsers.IsSuccessful())
        {
            return allUsers.GetValue()!.Select(u => new UserDto(u)).ToArray();
        }
        
        return (ObjectResult) allUsers;
    }

    [HttpPatch]
    [Route("{id:int}")]
    public ActionResult<UserDto> UpdateUser(
        [FromRoute] int id, 
        [FromBody] UpdateUser userUpdate,
        [FromServices] UserService userService)
    {
        var updatedUser = userService.UpdateUser(id, userUpdate);

        if (updatedUser.IsSuccessful())
        {
            return new UserDto(updatedUser.GetValue()!);
        }

        return (ObjectResult)updatedUser;
    }

    [HttpDelete]
    [Route("{userId:int}")]
    public ActionResult<bool> DeleteUser(
        [FromRoute] int userId,
        [FromServices] UserService userService)
    {
        var deletedUser = userService.DeleteUser(userId);

        if (deletedUser.IsSuccessful())
        {
            return  deletedUser.GetValue()!;
        }
        return (ObjectResult) deletedUser;
    }
}

