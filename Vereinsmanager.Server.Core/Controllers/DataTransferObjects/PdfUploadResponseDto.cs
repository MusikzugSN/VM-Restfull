namespace Vereinsmanager.Controllers.DataTransferObjects;

public record PdfUploadResponseDto(
    List<PdfUploadResponseFileDto> Files
);