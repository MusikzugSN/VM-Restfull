using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/musicSheet")]
public class MusicSheetController : ControllerBase
{
    [HttpGet("score/{scoreId:int}/voice/{voiceId:int}")]
    public ActionResult<MusicSheetDto[]> GetMusicSheet(
        [FromRoute] int scoreId,
        [FromRoute] int voiceId,
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

    [HttpGet("folder/{folderId:int}")]
    public ActionResult<MusicSheetDto[]> GetMusicSheets(
        [FromRoute] int folderId,
        [FromQuery] int[] voiceIds,
        [FromServices] MusicSheetService musicSheetService)
    {
        var sheetsResult = musicSheetService.ListMusicSheets(folderId, voiceIds);

        if (sheetsResult.IsSuccessful())
        {
            return sheetsResult.GetValue()!
                .Select(sheet => new MusicSheetDto(sheet))
                .ToArray();
        }

        return (ObjectResult)sheetsResult;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public ActionResult<MusicSheetDto> CreateMusicSheet(
        [FromForm] CreateMusicSheetRequestDto request,
        [FromServices] MusicSheetService musicSheetService)
    {
        if (request.ScoreId <= 0)
        {
            return BadRequest("scoreId ist ungültig.");
        }

        if (request.VoiceId <= 0)
        {
            return BadRequest("voiceId ist ungültig.");
        }

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("Es wurde keine gültige PDF-Datei übergeben.");
        }

        if (!request.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest($"Die Datei '{request.File.FileName}' ist keine PDF.");
        }

        var createdResult = musicSheetService.CreateMusicSheet(request);

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
    
    [HttpPut("{musicSheetId:int}/pdf")]
    [Consumes("multipart/form-data")]
    public ActionResult<MusicSheetDto> UpdateMusicSheetPdf(
        [FromRoute] int musicSheetId,
        [FromForm] IFormFile file,
        [FromServices] MusicSheetService musicSheetService)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Es wurde keine gültige Datei übergeben.");
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest($"Die Datei '{file.FileName}' ist keine PDF.");
        }

        var result = musicSheetService.UpdateMusicSheetPdf(musicSheetId, file);

        if (result.IsSuccessful())
        {
            return new MusicSheetDto(result.GetValue()!);
        }

        return (ObjectResult)result;
    }
}