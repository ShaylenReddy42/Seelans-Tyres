using SeelansTyres.Gateways.MvcBff.Services; // ITokenExchangeService

namespace SeelansTyres.Gateways.MvcBff.DelegatingHandlers;

public class AddressServiceDelegatingHandler : DelegatingHandler
{
    private readonly ITokenExchangeService tokenExchangeService;
    private readonly ILogger<AddressServiceDelegatingHandler> logger;

    public AddressServiceDelegatingHandler(
        ITokenExchangeService tokenExchangeService,
        ILogger<AddressServiceDelegatingHandler> logger)
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
            "DELEGATING HANDLER HIT", "AddressServiceDelegatingHandler");

        // Gets 'AddressService' as audience in the exchanged access token
        var additionalScopes = "AddressService.fullaccess";
        
        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
