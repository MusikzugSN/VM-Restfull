using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/instrument")]
public class InstrumentController : ControllerBase
{
    [HttpGet]
    public ActionResult<InstrumentDto[]> GetInstruments(
        [FromQuery] bool includeVoices,
        [FromServices] InstrumentService instrumentService)
    {
        var instrumentsResult = instrumentService.ListInstruments(includeVoices);

        if (instrumentsResult.IsSuccessful())
        {
            return instrumentsResult.GetValue()
                .Select(instrument => new InstrumentDto(instrument))
                .ToArray();
        }

        return (ObjectResult)instrumentsResult;
    }

    [HttpGet("{instrumentId:int}")]
    public ActionResult<InstrumentDto> GetInstrumentById(
        [FromRoute] int instrumentId,
        [FromQuery] bool includeVoices,
        [FromServices] InstrumentService instrumentService)
    {
        var instrumentResult = instrumentService.GetInstrumentById(instrumentId, includeVoices);

        if (instrumentResult.IsSuccessful())
        {
            return new InstrumentDto(instrumentResult.GetValue());
        }

        return (ObjectResult)instrumentResult;
    }

    [HttpPost]
    public ActionResult<InstrumentDto> CreateInstrument(
        [FromBody] CreateInstrument createInstrument,
        [FromServices] InstrumentService instrumentService)
    {
        var createdResult = instrumentService.CreateInstrument(createInstrument);

        if (createdResult.IsSuccessful())
        {
            return new InstrumentDto(createdResult.GetValue());
        }

        return (ObjectResult)createdResult;
    }

    [HttpPatch("{instrumentId:int}")]
    public ActionResult<InstrumentDto> UpdateInstrument(
        [FromRoute] int instrumentId,
        [FromBody] UpdateInstrument updateInstrument,
        [FromServices] InstrumentService instrumentService)
    {
        var updatedResult = instrumentService.UpdateInstrument(instrumentId, updateInstrument);

        if (updatedResult.IsSuccessful())
        {
            return new InstrumentDto(updatedResult.GetValue());
        }

        return (ObjectResult)updatedResult;
    }

    [HttpDelete("{instrumentId:int}")]
    public ActionResult<bool> DeleteInstrument(
        [FromRoute] int instrumentId,
        [FromServices] InstrumentService instrumentService)
    {
        var deletedResult = instrumentService.DeleteInstrument(instrumentId);

        if (deletedResult.IsSuccessful())
        {
            return deletedResult.GetValue();
        }

        return (ObjectResult)deletedResult;
    }
}