﻿using SeelansTyres.Gateways.MvcBff.Services; // ITokenExchangeService

namespace SeelansTyres.Gateways.MvcBff.DelegatingHandlers;

public class CustomerServiceFullAccessDelegatingHandler(
    ITokenExchangeService tokenExchangeService,
    ILogger<CustomerServiceFullAccessDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{announcement}: {delegatingHandler}",
            "DELEGATING HANDLER HIT", "CustomerServiceFullAccessDelegatingHandler");

        // Gets 'CustomerService' as audience in the exchanged access token
        var additionalScopes = "CustomerService.fullaccess";
        
        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
