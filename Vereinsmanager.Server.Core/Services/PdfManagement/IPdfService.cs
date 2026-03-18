using Vereinsmanager.Controllers.DataTransferObjects;
using Vereinsmanager.Services.Models;
using Vereinsmanager.Utils;

namespace Vereinsmanager.Services.PdfManagement;

public interface IPdfService
{
    ReturnValue<UploadPdfsResponseDto> UploadPdfs(UploadPdfsRequestDto request);
    string GetPdfPath(int scoreId, string fileId);
}