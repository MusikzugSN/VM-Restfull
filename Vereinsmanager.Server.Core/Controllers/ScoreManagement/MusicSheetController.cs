using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services;
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
    
    [HttpGet("status/{status:int}")]
    public ActionResult<MusicSheetDto[]> GetMusicSheetsByStatus(
        [FromRoute] int status,
        [FromQuery] int[] voiceIds,
        [FromServices] MusicSheetService musicSheetService)
    {
        var sheetsResult = musicSheetService.ListMusicSheetsByStatus(status, voiceIds);

        if (sheetsResult.IsSuccessful())
        {
            return sheetsResult.GetValue()!
                .Select(sheet => new MusicSheetDto(sheet))
                .ToArray();
        }

        return (ObjectResult)sheetsResult;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public ActionResult<List<MusicSheetDto>> CreateMusicSheet(
        [FromForm] CreateMusicSheetRequestDto request,
        [FromServices] MusicSheetService musicSheetService)
    {
        if (request.ScoreId <= 0)
            return BadRequest("scoreId ist ungültig.");

        

        if (request.Files == null || request.Files.Length == 0)
            return BadRequest("Es wurde keine gültige Datei übergeben.");

        foreach (var file in request.Files)
        {
            if (file.File == null)
                return BadRequest("Mindestens eine Datei ist ungültig.");

            if (file.VoiceId <= 0)
                return BadRequest("voiceId ist ungültig.");
            
            if (!IsSupportedUploadFile(file.File.FileName))
                return BadRequest($"Die Datei '{file.File.FileName}' ist nicht erlaubt.");
        }

        var createdResult = musicSheetService.CreateMusicSheets(request);

        if (createdResult.IsSuccessful())
            return createdResult.GetValue()!.Select(x => new MusicSheetDto(x)).ToList();

        return (ObjectResult)createdResult;
    }

    [HttpPost("cropByVoices")]
    [Consumes("multipart/form-data")]
    public ActionResult<MusicSheetDto[]> CropPdfByVoices(
        [FromForm] CropPdfByVoicesRequestDto request,
        [FromServices] MusicSheetService musicSheetService)
    {
        if (request.ScoreId <= 0)
            return BadRequest("scoreId ist ungültig.");

        if (request.File == null || request.File.Length == 0)
            return BadRequest("Es wurde keine gültige Datei übergeben.");

        if (!IsSupportedUploadFile(request.File.FileName))
            return BadRequest($"Die Datei '{request.File.FileName}' ist nicht erlaubt.");

        if (request.Ranges == null || request.Ranges.Count == 0)
            return BadRequest("Es wurden keine ranges übergeben.");

        foreach (var range in request.Ranges)
        {
            if (range.VoiceId <= 0)
                return BadRequest("Eine VoiceId ist ungültig.");

            if (range.FromPage <= 0 || range.ToPage <= 0)
                return BadRequest("Seitenzahlen müssen größer als 0 sein.");

            if (range.FromPage > range.ToPage)
                return BadRequest("FromPage darf nicht größer als ToPage sein.");
        }

        var result = musicSheetService.CropPdfByVoices(request);

        if (result.IsSuccessful())
        {
            return result.GetValue()!
                .Select(x => new MusicSheetDto(x))
                .ToArray();
        }

        return (ObjectResult)result;
    }

    [HttpPatch("{musicSheetId:int}")]
    public ActionResult<MusicSheetDto> UpdateMusicSheet(
        [FromRoute] int musicSheetId,
        [FromBody] UpdateMusicSheet updateMusicSheet,
        [FromServices] MusicSheetService musicSheetService)
    {
        var updatedResult = musicSheetService.UpdateMusicSheet(musicSheetId, updateMusicSheet);

        if (updatedResult.IsSuccessful())
            return new MusicSheetDto(updatedResult.GetValue()!);

        return (ObjectResult)updatedResult;
    }

    [HttpDelete("{musicSheetId:int}")]
    public ActionResult<bool> DeleteMusicSheet(
        [FromRoute] int musicSheetId,
        [FromServices] MusicSheetService musicSheetService)
    {
        var deletedResult = musicSheetService.DeleteMusicSheet(musicSheetId);

        if (deletedResult.IsSuccessful())
            return deletedResult.GetValue();

        return (ObjectResult)deletedResult;
    }

    [HttpPut("{musicSheetId:int}/files")]
    [Consumes("multipart/form-data")]
    public ActionResult<MusicSheetDto> ReplaceMusicSheetFiles(
        [FromRoute] int musicSheetId,
        [FromForm] IFormFile file,
        [FromServices] MusicSheetService musicSheetService)
    {
        
        if (!IsSupportedUploadFile(file.FileName))
            return BadRequest($"Die Datei '{file.FileName}' ist nicht erlaubt.");


        var result = musicSheetService.ReplaceMusicSheetFile(musicSheetId, file);

        if (result.IsSuccessful())
            return new MusicSheetDto(result.GetValue()!);

        return (ObjectResult)result;
    }

    private static bool IsSupportedUploadFile(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();

        return ext == ".pdf"
            || ext == ".jpg"
            || ext == ".jpeg"
            || ext == ".png"
            || ext == ".bmp"
            || ext == ".gif";
    }
}