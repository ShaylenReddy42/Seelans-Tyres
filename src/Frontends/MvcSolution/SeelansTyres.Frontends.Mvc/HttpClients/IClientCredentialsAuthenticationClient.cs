namespace SeelansTyres.Frontends.Mvc.HttpClients;

/// <summary>
/// Provides functionality to retrieve an access token using client credentials 
/// to authenticate with IdentityServer4
/// </summary>
public interface IClientCredentialsAuthenticationClient
{
    /// <summary>
    /// Uses the client credential flow to get an access token from IdentityServer4 using specific scopes
    /// </summary>
    /// <param name="additionalScopes">Space-separated api scopes to request in order to access downstream apis</param>
    /// <returns>A task of nullable string containing a valid access token meant to be used with 'Bearer' authentication</returns>
    Task<string?> RetrieveAccessTokenAsync(string additionalScopes);
}
