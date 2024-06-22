using IdentityModel.Client;                    // GetDiscoveryDocumentAsync(), RequestClientCredentialsTokenAsync(), ClientCredentialsTokenRequest
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants

namespace SeelansTyres.Frontends.Mvc.HttpClients;

public class ClientCredentialsAuthenticationClient(
    HttpClient client,
    IConfiguration configuration,
    ILogger<ClientCredentialsAuthenticationClient> logger,
    Stopwatch stopwatch) : IClientCredentialsAuthenticationClient
{
    public async Task<string?> RetrieveAccessTokenAsync(string additionalScopes)
    {
        logger.LogInformation(
            "Attempting to retrieve an access token from IdentityServer4 using the client credentials flow with {AdditionalScopes} as additional scope(s)",
            additionalScopes);

        stopwatch.Start();

        var discoveryDocument = await client.GetDiscoveryDocumentAsync(configuration["IdentityServer"]);

        if (discoveryDocument.IsError)
        {
            stopwatch.Stop();

            logger.LogError(
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve the discovery document from IdentityServer4 was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds);
        }

        var tokenResponse =
            await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    ClientId = configuration["ClientCredentials:ClientId"]!,
                    ClientSecret = configuration["ClientCredentials:ClientSecret"],
                    Address = discoveryDocument.TokenEndpoint,
                    Scope = $"SeelansTyresWebBff.fullaccess {additionalScopes}"
                });

        stopwatch.Start();

        if (tokenResponse.IsError)
        {
            logger.LogError(
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve an access token was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            logger.LogInformation(
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve an access token completed successfully",
                LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds);
        }

        return tokenResponse.AccessToken;
    }
}
