#nullable enable
using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class InstrumentDto : MetaDataDto
{
    public int InstrumentId { get; init; }
    public string Name { get; init; }
    public string Type { get; init; }

    public InstrumentDto(Instrument instrument)
    {
        InstrumentId = instrument.InstrumentId;
        Name = instrument.Name;
        Type = instrument.Type;

        CreatedAt = instrument.CreatedAt;
        CreatedBy = instrument.CreatedBy;
        UpdatedAt = instrument.UpdatedAt;
        UpdatedBy = instrument.UpdatedBy;
    }
}