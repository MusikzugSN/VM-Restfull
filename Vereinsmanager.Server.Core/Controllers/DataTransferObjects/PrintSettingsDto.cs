using Vereinsmanager.Database.ScoreManagment;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class PrintSettingsDto : MetaDataDto
{
    public int PrintConfigId { get; init; }
    public int PageCount { get; init; }
    public PrintMode Mode { get; init; }
    public DuplexMode Duplex { get; init; }

    public int FileFormat { get; init; }

    public PrintSettingsDto(PrintSettings printSettings)
    {
        PrintConfigId = printSettings.PrintConfigId;
        PageCount = printSettings.PageCount;
        Mode = printSettings.Mode;
        Duplex = printSettings.Duplex;
        FileFormat = printSettings.FileFormat;

        CreatedAt = printSettings.CreatedAt;
        CreatedBy = printSettings.CreatedBy;
        UpdatedAt = printSettings.UpdatedAt;
        UpdatedBy = printSettings.UpdatedBy;
    }
}