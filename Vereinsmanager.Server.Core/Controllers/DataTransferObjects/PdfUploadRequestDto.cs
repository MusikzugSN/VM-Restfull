namespace Vereinsmanager.Controllers.DataTransferObjects;

public record PdfUploadRequestDto(
    int ScoreId,
    List<PdfUploadFileDto> Files
);