namespace Vereinsmanager.Services;

public record TokenData(string Claim, int UserId, DateTime ExpiresAt);

// AsSingletonService
public class CustomTokenService
{
    private readonly Dictionary<string, TokenData> _tokens = new();

    public string GenerateToken(string claim, int userId, TimeSpan? duration = null)
    {
        DeleteExpiredTokens();

        var token = GenerateRandomToken();

        var expiresAt = DateTime.UtcNow.Add(duration ?? TimeSpan.FromMinutes(5));

        _tokens[token] = new TokenData(claim, userId, expiresAt);

        return token;
    }

    public TokenData? ValidateToken(string token, string claim)
    {
        DeleteExpiredTokens();

        if (_tokens.TryGetValue(token, out var tokenData))
        {
            if (tokenData.Claim == claim && DateTime.UtcNow <= tokenData.ExpiresAt)
            {
                return tokenData;
            }
        }

        return null;
    }

    private string GenerateRandomToken()
    {
        var token = Guid.NewGuid().ToString();
        return _tokens.ContainsKey(token) ? GenerateRandomToken() : token;
    }

    private void DeleteExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = _tokens
            .Where(kvp => kvp.Value.ExpiresAt < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in expiredTokens)
        {
            _tokens.Remove(token);
        }
    }
}