using SeelansTyres.Gateways.MvcBff.Services; // ITokenExchangeService

namespace SeelansTyres.Gateways.MvcBff.DelegatingHandlers;

public class TyresServiceDelegatingHandler : DelegatingHandler
{
    private readonly ITokenExchangeService tokenExchangeService;
    private readonly ILogger<TyresServiceDelegatingHandler> logger;

    public TyresServiceDelegatingHandler(
        ITokenExchangeService tokenExchangeService,
        ILogger<TyresServiceDelegatingHandler> logger)
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
            "DELEGATING HANDLER HIT", "TyresServiceDelegatingHandler");
        
        // Gets 'TyresService' as audience in the exchanged access token
        var additionalScopes = "TyresService.fullaccess";
        
        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
