using System.Net.Http.Headers; // AuthenticationHeaderValue

namespace SeelansTyres.Gateways.WebBff.Services;

/// <summary>
/// Provides functionality to perform a token exchange to access a downstream api
/// </summary>
public interface ITokenExchangeService
{
    /// <summary>
    /// <para>
    ///     Extracts the incoming access token as the subject token for the token exchange and exchanges it for an<br/>
    ///     access token with a valid audience to a downstream api using the additional scopes
    /// </para>
    /// </summary>
    /// <param name="incomingRequest">Incoming HTTP Request used to extract the incoming access token for the token exchange</param>
    /// <param name="additionalScopes">Space-separated api scopes to request in order to access downstream apis</param>
    /// <returns>The new access token along with the 'Bearer' authentication scheme wrapped in an AuthenticationHeaderValue</returns>
    Task<AuthenticationHeaderValue> PerformTokenExchangeAsync(HttpRequestMessage incomingRequest, string additionalScopes);
}
