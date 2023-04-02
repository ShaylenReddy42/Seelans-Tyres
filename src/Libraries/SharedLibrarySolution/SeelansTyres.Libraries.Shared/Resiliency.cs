using Microsoft.Extensions.DependencyInjection; // IHttpClientBuilder, IServiceCollection
using Microsoft.Extensions.Logging;             // ILogger
using Polly;                                    // IAsyncPolicy, WaitAndRetryAsync()
using Polly.Extensions.Http;                    // HttpPolicyExtensions
using System.Net;                               // HttpStatusCode

namespace SeelansTyres.Libraries.Shared;

public static class Resiliency
{
    /// <summary>
    /// Provides a high level abstraction for adding resiliency policies on http clients to handle transient faults
    /// </summary>
    /// <remarks>
    ///     A transient fault is sometimes described as a hardware glitch that causes a request to fail<br/>
    ///     <br/>
    ///     However, trying the request a couple milliseconds later would yield a successful request
    /// </remarks>
    /// <typeparam name="TService">The strongly typed http client</typeparam>
    /// <param name="builder">The http client builder</param>
    /// <param name="services">The service collection used further down the abstraction to extract a logger</param>
    /// <returns></returns>
    public static IHttpClientBuilder AddCommonResiliencyPolicies<TService>(
        this IHttpClientBuilder builder,
        IServiceCollection services) where TService : class
    {
        builder
            .AddPolicyHandler(RetryPolicy<TService>(services));
        
        return builder;
    }

    /// <summary>
    /// The retry resiliency policy that waits and retries based on hard-coded sleep durations
    /// </summary>
    /// <typeparam name="TService">The strongly typed http client the policy is added to</typeparam>
    /// <param name="services">The service collection used to extract a logger for logging errors on retry</param>
    /// <returns></returns>
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
