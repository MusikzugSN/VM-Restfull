namespace Vereinsmanager.Controllers.DataTransferObjects;

public record PdfUploadResponseFileDto(
    string FileName,
    string Guid
);