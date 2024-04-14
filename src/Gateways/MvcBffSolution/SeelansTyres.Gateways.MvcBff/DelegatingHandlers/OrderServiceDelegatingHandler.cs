using SeelansTyres.Gateways.MvcBff.Services; // ITokenExchangeService

namespace SeelansTyres.Gateways.MvcBff.DelegatingHandlers;

public class OrderServiceDelegatingHandler(
    ITokenExchangeService tokenExchangeService,
    ILogger<OrderServiceDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{Announcement}: {DelegatingHandler}",
            "DELEGATING HANDLER HIT", "OrderServiceDelegatingHandler");

        // Gets 'OrderService' as audience in the exchanged access token
        var additionalScopes = "OrderService.fullaccess";
        
        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
