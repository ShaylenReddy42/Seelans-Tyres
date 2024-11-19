﻿using SeelansTyres.Gateways.WebBff.Services; // ITokenExchangeService

namespace SeelansTyres.Gateways.WebBff.DelegatingHandlers;

public class TyresServiceDelegatingHandler(
    ITokenExchangeService tokenExchangeService,
    ILogger<TyresServiceDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{Announcement}: {DelegatingHandler}",
            "DELEGATING HANDLER HIT", "TyresServiceDelegatingHandler");

        // Gets 'TyresService' as audience in the exchanged access token
        var additionalScopes = "TyresService.fullaccess";

        request.Headers.Authorization = await tokenExchangeService.PerformTokenExchangeAsync(request, additionalScopes);

        return await base.SendAsync(request, cancellationToken);
    }
}