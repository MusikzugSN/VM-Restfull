#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Services.ScoreManagement;
using Vereinsmanager.Controllers.DataTransferObjects;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/musicFolder")]
public class MusicFolderController : ControllerBase
{
    [HttpGet]
    public ActionResult<MusicFolderDto[]> GetMusicFolders([FromServices] MusicFolderService musicFolderService)
    {
        var folders = musicFolderService.ListMusicFolders();

        if (folders.IsSuccessful())
        {
            return folders.GetValue()!
                .Select(f => new MusicFolderDto(f))
                .ToArray();
        }

        return (ObjectResult)folders;
    }

    [HttpPost]
    public ActionResult<MusicFolderDto> CreateMusicFolder(
        [FromBody] CreateMusicFolder createMusicFolder,
        [FromServices] MusicFolderService musicFolderService)
    {
        var newFolder = musicFolderService.CreateMusicFolder(createMusicFolder);

        if (newFolder.IsSuccessful())
        {
            return new MusicFolderDto(newFolder.GetValue()!);
        }

        return (ObjectResult)newFolder;
    }

    [HttpPatch]
    [Route("{musicFolderId:int}")]
    public ActionResult<MusicFolderDto> UpdateMusicFolder(
        [FromRoute] int musicFolderId,
        [FromBody] UpdateMusicFolder updateMusicFolder,
        [FromServices] MusicFolderService musicFolderService)
    {
        var updatedFolder = musicFolderService.UpdateMusicFolder(musicFolderId, updateMusicFolder);

        if (updatedFolder.IsSuccessful())
        {
            return new MusicFolderDto(updatedFolder.GetValue()!);
        }

        return (ObjectResult)updatedFolder;
    }

    [HttpDelete]
    [Route("{musicFolderId:int}")]
    public ActionResult<bool> DeleteMusicFolder(
        [FromRoute] int musicFolderId,
        [FromServices] MusicFolderService musicFolderService)
    {
        var deleted = musicFolderService.DeleteMusicFolder(musicFolderId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }

    [HttpGet]
    [Route("{musicFolderId:int}/scores")]
    public ActionResult<ScoreMusicFolderDto[]> ListScoresInFolder(
        [FromRoute] int musicFolderId,
        [FromQuery] bool includeScore,
        [FromServices] MusicFolderService musicFolderService)
    {
        var scores = musicFolderService.ListScoresInFolder(musicFolderId, includeScore);

        if (scores.IsSuccessful())
        {
            return scores.GetValue()!
                .Select(sm => new ScoreMusicFolderDto(sm))
                .ToArray();
        }

        return (ObjectResult)scores;
    }

    [HttpPost]
    [Route("{musicFolderId:int}/scores")]
    public ActionResult<ScoreMusicFolderDto> AddScoreToFolder(
        [FromRoute] int musicFolderId,
        [FromBody] AddScoreToMusicFolder addScoreToMusicFolder,
        [FromServices] MusicFolderService musicFolderService)
    {
        var added = musicFolderService.AddScoreToFolder(musicFolderId, addScoreToMusicFolder);

        if (added.IsSuccessful())
        {
            return new ScoreMusicFolderDto(added.GetValue()!);
        }

        return (ObjectResult)added;
    }

    [HttpPatch]
    [Route("scores/{scoreMusicFolderId:int}")]
    public ActionResult<ScoreMusicFolderDto> UpdateScoreMusicFolder(
        [FromRoute] int scoreMusicFolderId,
        [FromBody] UpdateScoreMusicFolder updateScoreMusicFolder,
        [FromServices] MusicFolderService musicFolderService)
    {
        var updated = musicFolderService.UpdateScoreMusicFolder(scoreMusicFolderId, updateScoreMusicFolder);

        if (updated.IsSuccessful())
        {
            return new ScoreMusicFolderDto(updated.GetValue()!);
        }

        return (ObjectResult)updated;
    }

    [HttpDelete]
    [Route("scores/{scoreMusicFolderId:int}")]
    public ActionResult<bool> DeleteScoreMusicFolder(
        [FromRoute] int scoreMusicFolderId,
        [FromServices] MusicFolderService musicFolderService)
    {
        var deleted = musicFolderService.DeleteScoreMusicFolder(scoreMusicFolderId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }
}