#nullable enable
using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects.Base;
using Vereinsmanager.Services;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/voice")]
public class VoiceController : ControllerBase
{
    [HttpGet]
    public ActionResult<VoiceDto[]> GetVoices(
        [FromQuery] bool includeInstrument,
        [FromQuery] bool includeAlternateVoices,
        [FromServices] VoiceService voiceService)
    {
        var voices = voiceService.ListVoices(includeInstrument, includeAlternateVoices);

        if (voices.IsSuccessful())
        {
            return voices.GetValue()!
                .Select(v => new VoiceDto(v))
                .ToArray();
        }

        return (ObjectResult)voices;
    }

    [HttpPost]
    public ActionResult<VoiceDto> CreateVoice(
        [FromBody] CreateVoice createVoice,
        [FromServices] VoiceService voiceService)
    {
        var newVoice = voiceService.CreateVoice(createVoice);

        if (newVoice.IsSuccessful())
        {
            return new VoiceDto(newVoice.GetValue()!);
        }

        return (ObjectResult)newVoice;
    }

    [HttpPatch]
    [Route("{voiceId:int}")]
    public ActionResult<VoiceDto> UpdateVoice(
        [FromRoute] int voiceId,
        [FromBody] UpdateVoice updateVoice,
        [FromServices] VoiceService voiceService)
    {
        var updated = voiceService.UpdateVoice(voiceId, updateVoice);

        if (updated.IsSuccessful())
        {
            return new VoiceDto(updated.GetValue()!);
        }

        return (ObjectResult)updated;
    }

    [HttpDelete]
    [Route("{voiceId:int}")]
    public ActionResult<bool> DeleteVoice(
        [FromRoute] int voiceId,
        [FromServices] VoiceService voiceService)
    {
        var deleted = voiceService.DeleteVoice(voiceId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }


    [HttpGet]
    [Route("{voiceId:int}/alternateVoices")]
    public ActionResult<AlternateVoiceDto[]> ListAlternateVoices(
        [FromRoute] int voiceId,
        [FromServices] VoiceService voiceService)
    {
        var alts = voiceService.ListAlternateVoices(voiceId);

        if (alts.IsSuccessful())
        {
            return alts.GetValue()!
                .Select(av => new AlternateVoiceDto(av))
                .ToArray();
        }

        return (ObjectResult)alts;
    }

    [HttpPost]
    [Route("{voiceId:int}/alternateVoices")]
    public ActionResult<AlternateVoiceDto> AddAlternateVoice(
        [FromRoute] int voiceId,
        [FromBody] CreateAlternateVoice createAlternateVoice,
        [FromServices] VoiceService voiceService)
    {
        var added = voiceService.AddAlternateVoice(voiceId, createAlternateVoice);

        if (added.IsSuccessful())
        {
            return new AlternateVoiceDto(added.GetValue()!);
        }

        return (ObjectResult)added;
    }

    [HttpPatch]
    [Route("{voiceId:int}/alternateVoices/{alternateVoiceId:int}")]
    public ActionResult<AlternateVoiceDto> UpdateAlternateVoice(
        [FromRoute] int voiceId,
        [FromRoute] int alternateVoiceId,
        [FromBody] UpdateAlternateVoice updateAlternateVoice,
        [FromServices] VoiceService voiceService)
    {
        var updated = voiceService.UpdateAlternateVoice(voiceId, alternateVoiceId, updateAlternateVoice);

        if (updated.IsSuccessful())
        {
            return new AlternateVoiceDto(updated.GetValue()!);
        }

        return (ObjectResult)updated;
    }

    [HttpDelete]
    [Route("{voiceId:int}/alternateVoices/{alternateVoiceId:int}")]
    public ActionResult<bool> DeleteAlternateVoice(
        [FromRoute] int voiceId,
        [FromRoute] int alternateVoiceId,
        [FromServices] VoiceService voiceService)
    {
        var deleted = voiceService.DeleteAlternateVoice(voiceId, alternateVoiceId);

        if (deleted.IsSuccessful())
        {
            return deleted.GetValue();
        }

        return (ObjectResult)deleted;
    }
}