namespace Vereinsmanager.Controllers.DataTransferObjects;

public record PdfPagePlacementDto(
    int TargetPageIndex,
    int SourcePageIndex,
    float X,
    float Y,
    float Width,
    float Height,
    float Rotation = 0,
    bool IsNormalized = true,
    bool KeepAspectRatio = true
);