using SeelansTyres.Gateways.MvcBff.Services;

namespace SeelansTyres.Gateways.MvcBff.DelegatingHandlers;

public class TyresServiceDelegatingHandler : DelegatingHandler
{
    private readonly ITokenExchangeService tokenExchangeService;

    public TyresServiceDelegatingHandler(ITokenExchangeService tokenExchangeService) =>
        this.tokenExchangeService = tokenExchangeService;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var incomingAccessToken = request.Headers.Authorization!.Parameter;

        var additionalScopes = "TyresService.fullaccess";
        
        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(incomingAccessToken!, additionalScopes);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
