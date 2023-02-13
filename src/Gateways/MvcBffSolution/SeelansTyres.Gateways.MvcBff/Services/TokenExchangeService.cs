using IdentityModel.Client;
using System.Net.Http.Headers;

namespace SeelansTyres.Gateways.MvcBff.Services;

public class TokenExchangeService : ITokenExchangeService
{
    private readonly HttpClient client;
    private readonly IConfiguration configuration;
    private readonly ILogger<TokenExchangeService> logger;

    public TokenExchangeService(
        HttpClient client,
        IConfiguration configuration,
        ILogger<TokenExchangeService> logger) => 
            (this.client, this.configuration, this.logger) = (client, configuration, logger);

    public async Task<AuthenticationHeaderValue> PerformTokenExchangeAsync(HttpRequestMessage incomingRequest, string additionalScopes)
    {
        logger.LogInformation(
            "Token exchange request triggered with {additionalScopes} as requested scope(s)",
            additionalScopes);
        
        var incomingAccessToken = incomingRequest.Headers.Authorization!.Parameter;

        var discoveryDocument = await client.GetDiscoveryDocumentAsync();

        if (discoveryDocument.IsError is true)
        {
            logger.LogError(
                "{announcement}: Attempt to retrieve IdentityServer4's discovery document was unsuccessful",
                "FAILED");

            logger.LogError(
                "True reason for failure: {IdentityServer4Error}",
                discoveryDocument.Error);
        }

        var tokenResponse = await client.RequestTokenExchangeTokenAsync(new TokenExchangeTokenRequest
        {
            Address = discoveryDocument.TokenEndpoint,
            SubjectTokenType = "urn:ietf:params:oauth:token-type:access_token",
            SubjectToken = incomingAccessToken,
            Scope = $"openid profile role {additionalScopes}",
            ClientId = configuration["ClientCredentials:ClientId"],
            ClientSecret = configuration["ClientCredentials:ClientSecret"]
        });

        if (tokenResponse.IsError is true)
        {
            logger.LogError(
                "{announcement}: Token exchange request was unsuccessful",
                "FAILED");
        }
        else
        {
            logger.LogInformation("Token exchange request completed successfully");
        }

        return new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
    }
}
