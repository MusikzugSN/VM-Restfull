namespace Vereinsmanager.Controllers.DataTransferObjects;

public record PdfLayoutDto(
    string SourceGuid,
    string TargetFormat,
    int TargetPageCount,
    List<PdfPagePlacementDto> Placements
);