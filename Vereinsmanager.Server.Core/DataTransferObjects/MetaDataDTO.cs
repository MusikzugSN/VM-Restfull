#nullable enable
namespace Vereinsmanager.DataTransferObjects;

public class MetaDataDTO
{
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}