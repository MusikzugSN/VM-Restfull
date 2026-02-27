#nullable enable
namespace Vereinsmanager.Utils;

public class OAuthConfig
{
    public required string ProviderKey { get; set; }
    public required string DisplayName { get; set; }
    public required string IssuerUrl { get; set; }
    public required string ClientId { get; set; }
}