#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.DataTransferObjects;
using Vereinsmanager.DataTransferObjects.Base;
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

        var problemDetails = newUser.GetProblemDetails();
        return StatusCode(problemDetails?.Status ?? 500, problemDetails?.Title ?? "Unknown Error");
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
        
        var problemDetials = updatedUser.GetProblemDetails();
        return StatusCode(problemDetials?.Status ?? 500, problemDetials?.Title ?? "Unknown Error");
    }
    
    
}

