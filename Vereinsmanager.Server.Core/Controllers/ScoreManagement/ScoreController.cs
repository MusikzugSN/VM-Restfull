using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/score")]
public class ScoreController : ControllerBase
{
    [HttpGet]
    public ActionResult<ScoreDto[]> GetScores([FromServices] ScoreService scoreService)
    {
        var scoresResult = scoreService.ListScores();

        if (scoresResult.IsSuccessful())
        {
            return scoresResult.GetValue()
                .Select(score => new ScoreDto(score))
                .ToArray();
        }

        return (ObjectResult)scoresResult;
    }
    [HttpGet("{scoreId:int}")]
    public ActionResult<ScoreDto> GetScoreById(
        [FromRoute] int scoreId,
        [FromServices] ScoreService scoreService)
    {
        var scoreResult = scoreService.LoadScoreById(scoreId);

        if (scoreResult == null)
        {
            return NotFound();
        }

        return new ScoreDto(scoreResult);
    }

    [HttpPost]
    public ActionResult<ScoreDto> CreateScore(
        [FromBody] CreateScore createScore,
        [FromServices] ScoreService scoreService)
    {
        var createdResult = scoreService.CreateScore(createScore);

        if (createdResult.IsSuccessful())
        {
            return new ScoreDto(createdResult.GetValue());
        }

        return (ObjectResult)createdResult;
    }

    [HttpPatch]
    [Route("{scoreId:int}")]
    public ActionResult<ScoreDto> UpdateScore(
        [FromRoute] int scoreId,
        [FromBody] UpdateScore updateScore,
        [FromServices] ScoreService scoreService)
    {
        var updatedResult = scoreService.UpdateScore(scoreId, updateScore);

        if (updatedResult.IsSuccessful())
        {
            return new ScoreDto(updatedResult.GetValue());
        }

        return (ObjectResult)updatedResult;
    }

    [HttpDelete]
    [Route("{scoreId:int}")]
    public ActionResult<bool> DeleteScore(
        [FromRoute] int scoreId,
        [FromServices] ScoreService scoreService)
    {
        var deletedResult = scoreService.DeleteScore(scoreId);

        if (deletedResult.IsSuccessful())
        {
            return deletedResult.GetValue();
        }

        return (ObjectResult)deletedResult;
    }
}