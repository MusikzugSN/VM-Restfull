using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Vereinsmanager.Utils;

public static class JwksLoader
{
    public static async Task<IEnumerable<SecurityKey>> LoadKeysAsync(string issuer)
    {
        var retriever = new HttpDocumentRetriever { RequireHttps = false };
        
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{issuer}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            retriever);

        var config = await configManager.GetConfigurationAsync(default);

        return config.SigningKeys;
    }
}