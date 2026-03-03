using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/voice")]
public class VoiceController : ControllerBase
{
    private readonly VoiceService _voiceService;

    public VoiceController(VoiceService voiceService)
    {
        _voiceService = voiceService;
    }

    [HttpGet]
    public ActionResult<VoiceDto[]> GetVoices(
        [FromQuery] bool includeInstrument = true,
        [FromQuery] bool includeAlternateVoices = true)
    {
        var voices = _voiceService.ListVoices(includeInstrument, includeAlternateVoices);

        if (voices.IsSuccessful())
        {
            return voices.GetValue()!
                .Select(v => new VoiceDto(v))
                .ToArray();
        }

        return (ObjectResult)voices;
    }

    [HttpGet("{voiceId:int}")]
    public ActionResult<VoiceDto> GetVoice(int voiceId)
    {
        var voice = _voiceService.GetVoiceById(voiceId, true, true);

        if (voice.IsSuccessful())
            return new VoiceDto(voice.GetValue()!);

        return (ObjectResult)voice;
    }

    [HttpPost]
    public ActionResult<VoiceDto> CreateVoice([FromBody] CreateVoice createVoice)
    {
        var result = _voiceService.CreateVoice(createVoice);

        if (result.IsSuccessful())
            return new VoiceDto(result.GetValue()!);

        return (ObjectResult)result;
    }

    [HttpPatch("{voiceId:int}")]
    public ActionResult<VoiceDto> UpdateVoice(int voiceId, [FromBody] UpdateVoice updateVoice)
    {
        var result = _voiceService.UpdateVoice(voiceId, updateVoice);

        if (result.IsSuccessful())
            return new VoiceDto(result.GetValue()!);

        return (ObjectResult)result;
    }

    [HttpDelete("{voiceId:int}")]
    public ActionResult<bool> DeleteVoice(int voiceId)
    {
        var result = _voiceService.DeleteVoice(voiceId);

        if (result.IsSuccessful())
            return result.GetValue();

        return (ObjectResult)result;
    }
}