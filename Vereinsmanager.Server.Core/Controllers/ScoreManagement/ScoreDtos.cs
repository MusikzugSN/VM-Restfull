namespace Vereinsmanager.Services.Models.DTOs;

public class ScoreDtos
{
    
}
public record CreateScore(
    string Title,
    string Composer,
    string Link,
    int Duration
);

public record UpdateScore(
    string? Title,
    string? Composer,
    string? Link,
    int? Duration
);

public record ScoreDto(
    int ScoreId,
    string Title,
    string Composer,
    string Link,
    int Duration
);