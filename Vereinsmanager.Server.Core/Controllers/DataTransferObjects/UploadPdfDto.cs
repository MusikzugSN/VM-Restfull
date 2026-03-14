namespace Vereinsmanager.Controllers.DataTransferObjects;

public record UploadPdfDto(
    string FileId,
    string OriginalFileName,
    long FileSize
);