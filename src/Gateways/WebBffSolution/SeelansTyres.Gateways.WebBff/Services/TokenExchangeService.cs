﻿using IdentityModel.Client;                    // GetDiscoveryDocumentAsync(), RequestTokenExchangeTokenAsync(), TokenExchangeTokenRequest
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants
using System.Net.Http.Headers;                 // AuthenticationHeaderValue

namespace SeelansTyres.Gateways.WebBff.Services;

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
            "Token exchange request triggered with {AdditionalScopes} as requested scope(s)",
            additionalScopes);

        var incomingAccessToken = incomingRequest.Headers.Authorization!.Parameter;

        var discoveryDocument = await client.GetDiscoveryDocumentAsync();

        if (discoveryDocument.IsError)
        {
            logger.LogError(
                "{Announcement}: Attempt to retrieve IdentityServer4's discovery document was unsuccessful. Reason: {IdentityServer4Error}",
                LoggerConstants.FailedAnnouncement, discoveryDocument.Error);
        }

        var tokenResponse = await client.RequestTokenExchangeTokenAsync(new TokenExchangeTokenRequest
        {
            Address = discoveryDocument.TokenEndpoint,
            SubjectTokenType = "urn:ietf:params:oauth:token-type:access_token",
            SubjectToken = incomingAccessToken!,
            Scope = $"openid profile role {additionalScopes}",
            ClientId = configuration["ClientCredentials:ClientId"]!,
            ClientSecret = configuration["ClientCredentials:ClientSecret"]
        });

        if (tokenResponse.IsError)
        {
            logger.LogError(
                "{Announcement}: Token exchange request was unsuccessful",
                LoggerConstants.FailedAnnouncement);
        }
        else
        {
            logger.LogInformation("Token exchange request completed successfully");
        }

        return new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
    }
}
