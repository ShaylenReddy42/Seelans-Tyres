using System.Net.Http.Headers;

namespace SeelansTyres.Gateways.MvcBff.Services;

public interface ITokenExchangeService
{
    Task<AuthenticationHeaderValue> PerformTokenExchangeAsync(string incomingAccessToken, string additionalScopes);
}
