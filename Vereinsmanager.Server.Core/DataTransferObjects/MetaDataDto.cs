#nullable enable
namespace Vereinsmanager.DataTransferObjects;

public class MetaDataDto
{
    public string CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }

    public string UpdatedBy { get; init; }
    public DateTime UpdatedAt { get; init; }
}