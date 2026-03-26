using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Database.Base;
using Vereinsmanager.Services.ScoreManagement;


[ApiController]
[Route("api/v1/tag")]
public class TagsController : ControllerBase
{
    [HttpGet]
    public ActionResult<TagDto[]> GetTags(
        [FromQuery] bool includeTagUsers,
        [FromServices] TagService tagService,
        [FromServices] UserContext userContext)
    {
        var tagsResult = tagService.ListTags(includeTagUsers);

        if (tagsResult.IsSuccessful())
        {
            return tagsResult.GetValue()
                .Select(tag => new TagDto(tag))
                .ToArray();
        }

        return (ObjectResult)tagsResult;
    }

    [HttpGet("{tagId:int}")]
    public ActionResult<TagDto> GetTagById(
        [FromRoute] int tagId,
        [FromQuery] bool includeTagUsers,
        [FromServices] TagService tagService)
    {
        var tagResult = tagService.GetTagById(tagId, includeTagUsers);

        if (tagResult.IsSuccessful())
        {
            return new TagDto(tagResult.GetValue()!);
        }

        return (ObjectResult)tagResult;
    }

    [HttpPost]
    public ActionResult<TagDto> CreateTag(
        [FromBody] CreateTag createTag,
        [FromServices] TagService tagService)
    {
        var createdResult = tagService.CreateTag(createTag);

        if (createdResult.IsSuccessful())
        {
            return new TagDto(createdResult.GetValue()!);
        }

        return (ObjectResult)createdResult;
    }

    [HttpPatch("{tagId:int}")]
    public ActionResult<TagDto> UpdateTag(
        [FromRoute] int tagId,
        [FromBody] UpdateTag updateTag,
        [FromServices] TagService tagService)
    {
        var updatedResult = tagService.UpdateTag(tagId, updateTag);

        if (updatedResult.IsSuccessful())
        {
            return new TagDto(updatedResult.GetValue()!);
        }

        return (ObjectResult)updatedResult;
    }

    [HttpDelete("{tagId:int}")]
    public ActionResult<bool> DeleteTag(
        [FromRoute] int tagId,
        [FromServices] TagService tagService)
    {
        var deletedResult = tagService.DeleteTag(tagId);

        if (deletedResult.IsSuccessful())
        {
            return deletedResult.GetValue();
        }

        return (ObjectResult)deletedResult;
    }
    
    [HttpGet("forMyArea")]
      public ActionResult<TagDto[]> GetTagsForMyArea(
        [FromQuery] bool includeTags,
        [FromServices] TagService tagService)
    {
        var tagsResult = tagService.ListTags(includeTags);
    
        if (tagsResult.IsSuccessful())
        {
            return tagsResult.GetValue()!
                .Select(tag => new TagDto(tag))
                .ToArray();
        }
    
        return (ObjectResult)tagsResult;
    }
    
}
