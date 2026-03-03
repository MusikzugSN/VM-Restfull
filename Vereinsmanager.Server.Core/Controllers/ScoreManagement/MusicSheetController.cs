using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/musicSheet")]
public class MusicSheetController : ControllerBase
{
    [HttpGet]
    public ActionResult<MusicSheetDto[]> GetMusicSheets(
        [FromRoute] int? scoreId,
        [FromQuery] int? voiceId,
        [FromServices] MusicSheetService musicSheetService)
    {
        var sheetsResult = musicSheetService.ListMusicSheets(scoreId, voiceId);

        if (sheetsResult.IsSuccessful())
        {
            return sheetsResult.GetValue()!
                .Select(sheet => new MusicSheetDto(sheet))
                .ToArray();
        }

        return (ObjectResult)sheetsResult;
    }

    [HttpGet("{musicSheetId:int}")]

    [HttpPost]
    public ActionResult<MusicSheetDto> CreateMusicSheet(
        [FromBody] CreateMusicSheet createMusicSheet,
        [FromServices] MusicSheetService musicSheetService)
    {
        var createdResult = musicSheetService.CreateMusicSheet(createMusicSheet);

        if (createdResult.IsSuccessful())
        {
            return new MusicSheetDto(createdResult.GetValue()!);
        }

        return (ObjectResult)createdResult;
    }

    [HttpPatch("{musicSheetId:int}")]
    public ActionResult<MusicSheetDto> UpdateMusicSheet(
        [FromRoute] int musicSheetId,
        [FromBody] UpdateMusicSheet updateMusicSheet,
        [FromServices] MusicSheetService musicSheetService)
    {
        var updatedResult = musicSheetService.UpdateMusicSheet(musicSheetId, updateMusicSheet);

        if (updatedResult.IsSuccessful())
        {
            return new MusicSheetDto(updatedResult.GetValue()!);
        }

        return (ObjectResult)updatedResult;
    }

    [HttpDelete("{musicSheetId:int}")]
    public ActionResult<bool> DeleteMusicSheet(
        [FromRoute] int musicSheetId,
        [FromServices] MusicSheetService musicSheetService)
    {
        var deletedResult = musicSheetService.DeleteMusicSheet(musicSheetId);

        if (deletedResult.IsSuccessful())
        {
            return deletedResult.GetValue();
        }

        return (ObjectResult)deletedResult;
    }
}