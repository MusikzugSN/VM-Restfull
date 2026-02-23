#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services.Models;
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
        var instruments = instrumentService.ListInstruments(includeVoices);

        if (instruments.IsSuccessful())
        {
            return instruments.GetValue()!
                .Select(i => new InstrumentDto(i))
                .ToArray();
        }

        return (ObjectResult)instruments;
    }

    [HttpGet]
    [Route("{instrumentId:int}")]
    public ActionResult<InstrumentDto> GetInstrumentById(
        [FromRoute] int instrumentId,
        [FromQuery] bool includeVoices,
        [FromServices] InstrumentService instrumentService)
    {
        var instrument = instrumentService.GetInstrumentById(instrumentId, includeVoices);

        if (instrument.IsSuccessful())
        {
            return new InstrumentDto(instrument.GetValue()!);
        }

        return (ObjectResult)instrument;
    }

    [HttpPost]
    public ActionResult<InstrumentDto> CreateInstrument(
        [FromBody] CreateInstrument createInstrument,
        [FromServices] InstrumentService instrumentService)
    {
        var created = instrumentService.CreateInstrument(createInstrument);

        if (created.IsSuccessful())
        {
            return new InstrumentDto(created.GetValue()!);
        }

        return (ObjectResult)created;
    }

    [HttpPatch]
    [Route("{instrumentId:int}")]
    public ActionResult<InstrumentDto> UpdateInstrument(
        [FromRoute] int instrumentId,
        [FromBody] UpdateInstrument updateInstrument,
        [FromServices] InstrumentService instrumentService)
    {
        var updated = instrumentService.UpdateInstrument(instrumentId, updateInstrument);

        if (updated.IsSuccessful())
        {
            return new InstrumentDto(updated.GetValue()!);
        }

        return (ObjectResult)updated;
    }

    [HttpDelete]
    [Route("{instrumentId:int}")]
    public ActionResult<bool> DeleteInstrument(
        [FromRoute] int instrumentId,
        [FromServices] InstrumentService instrumentService)
    {
        var deleted = instrumentService.DeleteInstrument(instrumentId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }
}