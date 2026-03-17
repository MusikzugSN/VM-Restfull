using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Vereinsmanager.Utils;

public static class JwksLoader
{
    public static Dictionary<string, IEnumerable<SecurityKey>> CachedKeys = new();
    
    public static async Task<IEnumerable<SecurityKey>> LoadKeysAsync(string issuer)
    {
        if (CachedKeys.TryGetValue(issuer, out var keys))
            return keys;
        
        var retriever = new HttpDocumentRetriever { RequireHttps = issuer.StartsWith("https://") };
        
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{issuer}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            retriever);

        var config = await configManager.GetConfigurationAsync(default);
        
        var loadedKeys = config.SigningKeys;
        CachedKeys[issuer] = loadedKeys;
        
        return loadedKeys;
    }
}