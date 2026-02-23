#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/musicSheet")]
public class MusicSheetController : ControllerBase
{
    [HttpGet]
    public ActionResult<MusicSheetDto[]> ListMusicSheets(
        [FromQuery] int? scoreId,
        [FromQuery] int? voiceId,
        [FromQuery] bool includeScore,
        [FromQuery] bool includeVoice,
        [FromServices] MusicSheetService musicSheetService)
    {
        var sheets = musicSheetService.ListMusicSheets(
            scoreId: scoreId,
            voiceId: voiceId,
            includeScore: includeScore,
            includeVoice: includeVoice);

        if (sheets.IsSuccessful())
        {
            return sheets.GetValue()!
                .Select(ms => new MusicSheetDto(ms))
                .ToArray();
        }

        return (ObjectResult)sheets;
    }

    [HttpPost]
    public ActionResult<MusicSheetDto> CreateMusicSheet(
        [FromBody] CreateMusicSheet createMusicSheet,
        [FromServices] MusicSheetService musicSheetService)
    {
        var created = musicSheetService.CreateMusicSheet(createMusicSheet);

        if (created.IsSuccessful())
        {
            return new MusicSheetDto(created.GetValue()!);
        }

        return (ObjectResult)created;
    }

    [HttpPatch]
    [Route("{musicSheetId:int}")]
    public ActionResult<MusicSheetDto> UpdateMusicSheet(
        [FromRoute] int musicSheetId,
        [FromBody] UpdateMusicSheet updateMusicSheet,
        [FromServices] MusicSheetService musicSheetService)
    {
        var updated = musicSheetService.UpdateMusicSheet(musicSheetId, updateMusicSheet);

        if (updated.IsSuccessful())
        {
            return new MusicSheetDto(updated.GetValue()!);
        }

        return (ObjectResult)updated;
    }

    [HttpDelete]
    [Route("{musicSheetId:int}")]
    public ActionResult<bool> DeleteMusicSheet(
        [FromRoute] int musicSheetId,
        [FromServices] MusicSheetService musicSheetService)
    {
        var deleted = musicSheetService.DeleteMusicSheet(musicSheetId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }
}