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

    public async Task<AuthenticationHeaderValue> PerformTokenExchangeAsync(HttpRequestMessage incomingRequest, string additionalScopes)
    {
        var incomingAccessToken = incomingRequest.Headers.Authorization!.Parameter;

        var discoveryDocument = await client.GetDiscoveryDocumentAsync();

        var tokenResponse = await client.RequestTokenExchangeTokenAsync(new TokenExchangeTokenRequest
        {
            Address = discoveryDocument.TokenEndpoint,
            SubjectTokenType = "urn:ietf:params:oauth:token-type:access_token",
            SubjectToken = incomingAccessToken,
            Scope = $"openid profile {additionalScopes}",
            ClientId = configuration["ClientCredentials:ClientId"],
            ClientSecret = configuration["ClientCredentials:ClientSecret"]
        });

        return new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
    }
}
