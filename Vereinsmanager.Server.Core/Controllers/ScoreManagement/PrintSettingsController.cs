using Microsoft.AspNetCore.Mvc;
using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.ScoreManagement;

namespace Vereinsmanager.Controllers.ScoreManagement;

[ApiController]
[Route("api/v1/printconf")]
public class PrintSettingsController : ControllerBase
{
    [HttpGet]
    public ActionResult<PrintSettingsDto[]> GetPrintSettings(
        [FromServices] PrintSettingsService printSettingsService)
    {
        var result = printSettingsService.ListPrintSettings();

        if (result.IsSuccessful())
        {
            return result.GetValue()!
                .Select(item => new PrintSettingsDto(item))
                .ToArray();
        }

        return (ObjectResult)result;
    }

    [HttpGet("{printConfigId:int}")]
    public ActionResult<PrintSettingsDto> GetPrintSettingsById(
        [FromRoute] int printConfigId,
        [FromServices] PrintSettingsService printSettingsService)
    {
        var result = printSettingsService.GetPrintSettingsById(printConfigId);

        if (result.IsSuccessful())
        {
            return new PrintSettingsDto(result.GetValue()!);
        }

        return (ObjectResult)result;
    }

    [HttpPost]
    public ActionResult<PrintSettingsDto> CreatePrintSettings(
        [FromBody] CreatePrintSettings createPrintSettings,
        [FromServices] PrintSettingsService printSettingsService)
    {
        var result = printSettingsService.CreatePrintSettings(createPrintSettings);

        if (result.IsSuccessful())
        {
            return new PrintSettingsDto(result.GetValue()!);
        }

        return (ObjectResult)result;
    }

    [HttpPatch("{printConfigId:int}")]
    public ActionResult<PrintSettingsDto> UpdatePrintSettings(
        [FromRoute] int printConfigId,
        [FromBody] UpdatePrintSettings updatePrintSettings,
        [FromServices] PrintSettingsService printSettingsService)
    {
        var result = printSettingsService.UpdatePrintSettings(printConfigId, updatePrintSettings);

        if (result.IsSuccessful())
        {
            return new PrintSettingsDto(result.GetValue()!);
        }

        return (ObjectResult)result;
    }

    [HttpDelete("{printConfigId:int}")]
    public ActionResult<bool> DeletePrintSettings(
        [FromRoute] int printConfigId,
        [FromServices] PrintSettingsService printSettingsService)
    {
        var result = printSettingsService.DeletePrintSettings(printConfigId);

        if (result.IsSuccessful())
        {
            return result.GetValue();
        }

        return (ObjectResult)result;
    }
}

