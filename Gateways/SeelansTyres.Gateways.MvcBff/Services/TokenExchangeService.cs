using IdentityModel.Client;
using System.Net.Http.Headers;

namespace SeelansTyres.Gateways.MvcBff.Services;

public class TokenExchangeService : ITokenExchangeService
{
    private readonly HttpClient client;
    private readonly IConfiguration configuration;

    public TokenExchangeService(
        HttpClient client,
        IConfiguration configuration) =>
            (this.client, this.configuration) = (client, configuration);

    public async Task<AuthenticationHeaderValue> PerformTokenExchangeAsync(string incomingAccessToken, string additionalScopes)
    {
        var discoveryDocument = await client.GetDiscoveryDocumentAsync(configuration["IdentityServerUrl"]);

        var customParameters = new Parameters
        {
            new("subject_token_type", "urn:ietf:params:oauth:token-type:access_token"),
            new("subject_token", incomingAccessToken),
            new("scope", $"openid profile {additionalScopes}")
        };

        var tokenResponse = await client.RequestTokenAsync(new TokenRequest
        {
            Address = discoveryDocument.TokenEndpoint,
            Parameters = customParameters,
            GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
            ClientId = configuration["ClientCredentials:ClientId"],
            ClientSecret = configuration["ClientCredentials:ClientSecret"]
        });

        return new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
    }
}
