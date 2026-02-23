#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/score")]
public class ScoreController : ControllerBase
{
    [HttpGet]
    public ActionResult<ScoreDto[]> GetScores([FromServices] ScoreService scoreService)
    {
        var scores = scoreService.ListScores();

        if (scores.IsSuccessful())
        {
            return scores.GetValue()!
                .Select(s => new ScoreDto(s))
                .ToArray();
        }

        return (ObjectResult)scores;
    }

    [HttpPost]
    public ActionResult<ScoreDto> CreateScore(
        [FromBody] CreateScore createScore,
        [FromServices] ScoreService scoreService)
    {
        var newScore = scoreService.CreateScore(createScore);

        if (newScore.IsSuccessful())
        {
            return new ScoreDto(newScore.GetValue()!);
        }

        return (ObjectResult)newScore;
    }

    [HttpPatch]
    [Route("{scoreId:int}")]
    public ActionResult<ScoreDto> UpdateScore(
        [FromRoute] int scoreId,
        [FromBody] UpdateScore updateScore,
        [FromServices] ScoreService scoreService)
    {
        var updatedScore = scoreService.UpdateScore(scoreId, updateScore);

        if (updatedScore.IsSuccessful())
        {
            return new ScoreDto(updatedScore.GetValue()!);
        }

        return (ObjectResult)updatedScore;
    }

    [HttpDelete]
    [Route("{scoreId:int}")]
    public ActionResult<bool> DeleteScore(
        [FromRoute] int scoreId,
        [FromServices] ScoreService scoreService)
    {
        var deleted = scoreService.DeleteScore(scoreId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }
}