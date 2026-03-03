using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/musicFolder")]
public class MusicFolderController : ControllerBase
{
    [HttpGet]
    public ActionResult<MusicFolderDto[]> GetMusicFolders([FromServices] MusicFolderService musicFolderService)
    {
        var foldersResult = musicFolderService.ListMusicFolders();

        if (foldersResult.IsSuccessful())
        {
            return foldersResult.GetValue()!
                .Select(folder => new MusicFolderDto(folder))
                .ToArray();
        }

        return (ObjectResult)foldersResult;
    }

    [HttpGet("{musicFolderId:int}")]
    public ActionResult<MusicFolderDto> GetMusicFolderById(
        [FromRoute] int musicFolderId,
        [FromServices] MusicFolderService musicFolderService)
    {
        var folderResult = musicFolderService.GetMusicFolderById(musicFolderId);

        if (folderResult.IsSuccessful())
        {
            return new MusicFolderDto(folderResult.GetValue()!);
        }

        return (ObjectResult)folderResult;
    }

    [HttpPost]
    public ActionResult<MusicFolderDto> CreateMusicFolder(
        [FromBody] CreateMusicFolder createMusicFolder,
        [FromServices] MusicFolderService musicFolderService)
    {
        var createdResult = musicFolderService.CreateMusicFolder(createMusicFolder);

        if (createdResult.IsSuccessful())
        {
            return new MusicFolderDto(createdResult.GetValue()!);
        }

        return (ObjectResult)createdResult;
    }

    [HttpPatch("{musicFolderId:int}")]
    public ActionResult<MusicFolderDto> UpdateMusicFolder(
        [FromRoute] int musicFolderId,
        [FromBody] UpdateMusicFolder updateMusicFolder,
        [FromServices] MusicFolderService musicFolderService)
    {
        var updatedResult = musicFolderService.UpdateMusicFolder(musicFolderId, updateMusicFolder);

        if (updatedResult.IsSuccessful())
        {
            return new MusicFolderDto(updatedResult.GetValue()!);
        }

        return (ObjectResult)updatedResult;
    }

    [HttpDelete("{musicFolderId:int}")]
    public ActionResult<bool> DeleteMusicFolder(
        [FromRoute] int musicFolderId,
        [FromServices] MusicFolderService musicFolderService)
    {
        var deletedResult = musicFolderService.DeleteMusicFolder(musicFolderId);

        if (deletedResult.IsSuccessful())
        {
            return deletedResult.GetValue();
        }

        return (ObjectResult)deletedResult;
    }
}