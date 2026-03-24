using Microsoft.AspNetCore.Http;

namespace Vereinsmanager.Controllers.DataTransferObjects;

public class VoicePageRangeDto
{
    public int VoiceId { get; set; }
    public int FromPage { get; set; }
    public int ToPage { get; set; }
    
    public int ScoreId { get; set; }
}

public class CropPdfByVoicesRequestDto
{
    public IFormFile? File { get; set; }
    public List<VoicePageRangeDto> Ranges { get; set; } = [];
}