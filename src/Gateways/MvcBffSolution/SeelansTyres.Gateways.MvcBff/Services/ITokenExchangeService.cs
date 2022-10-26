using System.Net.Http.Headers;

namespace SeelansTyres.Gateways.MvcBff.Services;

public interface ITokenExchangeService
{
    Task<AuthenticationHeaderValue> PerformTokenExchangeAsync(HttpRequestMessage incomingRequest, string additionalScopes);
}
