using SeelansTyres.Gateways.MvcBff.Services; // ITokenExchangeService

namespace SeelansTyres.Gateways.MvcBff.DelegatingHandlers;

public class OrderServiceDelegatingHandler : DelegatingHandler
{
    private readonly ITokenExchangeService tokenExchangeService;
    private readonly ILogger<OrderServiceDelegatingHandler> logger;

    public OrderServiceDelegatingHandler(
        ITokenExchangeService tokenExchangeService,
        ILogger<OrderServiceDelegatingHandler> logger)
    {
        this.tokenExchangeService = tokenExchangeService;
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{announcement}: {delegatingHandler}",
            "DELEGATING HANDLER HIT", "OrderServiceDelegatingHandler");

        // Gets 'OrderService' as audience in the exchanged access token
        var additionalScopes = "OrderService.fullaccess";
        
        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
