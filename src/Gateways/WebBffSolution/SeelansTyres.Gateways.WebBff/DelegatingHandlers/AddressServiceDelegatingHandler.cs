using SeelansTyres.Gateways.WebBff.Services; // ITokenExchangeService

namespace SeelansTyres.Gateways.WebBff.DelegatingHandlers;

public class AddressServiceDelegatingHandler(
    ITokenExchangeService tokenExchangeService,
    ILogger<AddressServiceDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{Announcement}: {DelegatingHandler}",
            "DELEGATING HANDLER HIT", "AddressServiceDelegatingHandler");

        // Gets 'AddressService' as audience in the exchanged access token
        var additionalScopes = "AddressService.fullaccess";

        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);

        return await base.SendAsync(request, cancellationToken);
    }
}
