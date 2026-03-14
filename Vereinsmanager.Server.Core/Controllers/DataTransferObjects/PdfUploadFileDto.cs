namespace Vereinsmanager.Controllers.DataTransferObjects;

public record PdfUploadFileDto(
    string FileName,
    int VoiceId
);