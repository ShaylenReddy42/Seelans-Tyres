using SeelansTyres.Gateways.MvcBff.Services;

namespace SeelansTyres.Gateways.MvcBff.DelegatingHandlers;

public class CustomerServiceFullAccessDelegatingHandler : DelegatingHandler
{
    private readonly ITokenExchangeService tokenExchangeService;
    private readonly ILogger<CustomerServiceFullAccessDelegatingHandler> logger;

    public CustomerServiceFullAccessDelegatingHandler(
        ITokenExchangeService tokenExchangeService,
        ILogger<CustomerServiceFullAccessDelegatingHandler> logger)
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
            "DELEGATING HANDLER HIT", "CustomerServiceFullAccessDelegatingHandler");

        var additionalScopes = "CustomerService.fullaccess";
        
        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
