namespace Vereinsmanager.Controllers.DataTransferObjects;

public record PdfLayoutDto(
    string SourceFileId,
    string TargetFormat,
    int TargetPageCount,
    List<PdfPagePlacementDto> Placements
);