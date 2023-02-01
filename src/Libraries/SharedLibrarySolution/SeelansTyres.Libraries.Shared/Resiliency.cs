using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace SeelansTyres.Libraries.Shared;

public static class Resiliency
{
    public static IHttpClientBuilder AddCommonResiliencyPolicies<TService>(
        this IHttpClientBuilder builder,
        IServiceCollection services) where TService : class
    {
        builder
            .AddPolicyHandler(RetryPolicy<TService>(services));
        
        return builder;
    }

    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy<TService>(
        IServiceCollection services) where TService : class
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(httpResponseMessage => httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                sleepDurations: new[]
                {
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromMilliseconds(750)
                },
                onRetry: (outcome, timeSpan, retryAttempt, context) =>
                {
                    services
                        .BuildServiceProvider()
                        .GetRequiredService<ILogger<TService>>()?
                        .LogError(
                            "Delaying for {delay}ms, then making retry: {retry}",
                            timeSpan.TotalMilliseconds, retryAttempt);
                });
    }
}
