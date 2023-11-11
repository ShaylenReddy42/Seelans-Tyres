using HealthChecks.UI.Client;                        // UIResponseWriter
using Microsoft.AspNetCore.Builder;                  // WebApplication
using Microsoft.AspNetCore.Diagnostics.HealthChecks; // HealthCheckOptions
using Microsoft.EntityFrameworkCore;                 // DbContext, AddDbContextCheck()
using Microsoft.Extensions.DependencyInjection;      // IHealthChecksBuilder
using Microsoft.Extensions.Diagnostics.HealthChecks; // HealthCheckResult, HealthStatus
using SeelansTyres.Libraries.Shared.Models;          // HealthChecksModel

namespace SeelansTyres.Libraries.Shared.Abstractions;

public static class HealthChecks
{
    /// <summary>
    /// Add the health checks that are part of all applications in the solution
    /// </summary>
    /// <remarks>
    /// Those checks are:<br/>
    /// 1. A check on self<br/>
    /// 2. A check for Elasticsearch if it's enabled<br/>
    /// <br/>
    /// If Application Insights is enabled, the health check status is published to it
    /// </remarks>
    /// <param name="healthChecks">The health checks builder</param>
    /// <param name="healthChecksModel">The model containing the needed properties to configure the health checks</param>
    /// <returns></returns>
    public static IHealthChecksBuilder AddCommonChecks(this IHealthChecksBuilder healthChecks, HealthChecksModel healthChecksModel)
    {
        healthChecks
            .AddCheck(
                name: "self",
                check: () => HealthCheckResult.Healthy(),
                tags: new[] { "self" });

        if (healthChecksModel.EnableElasticsearchHealthCheck)
        {
            healthChecks
                .AddElasticsearch(
                    elasticsearchUri: healthChecksModel.ElasticsearchUrl,
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(1.5));
        }

        if (healthChecksModel.PublishHealthStatusToAppInsights)
        {
            healthChecks.AddApplicationInsightsPublisher();
        }

        return healthChecks;
    }

    /// <summary>
    /// Provides an abstraction to adding a dbcontext check
    /// </summary>
    /// <typeparam name="TDbContext">The dbcontext to have a health check</typeparam>
    /// <param name="healthChecks">The health checks builder</param>
    /// <returns>The health checks builder with the added health check</returns>
    public static IHealthChecksBuilder AddCommonDbContextCheck<TDbContext>(this IHealthChecksBuilder healthChecks) where TDbContext : DbContext
    {
        healthChecks.AddDbContextCheck<TDbContext>(
            name: "database",
            failureStatus: HealthStatus.Unhealthy);

        return healthChecks;
    }

    /// <summary>
    /// Provides an abstraction to adding an identity server check
    /// </summary>
    /// <param name="healthChecks">The health checks builder</param>
    /// <param name="identityServerUrl">The url of the identity server</param>
    /// <returns>The health checks builder with the added health check</returns>
    public static IHealthChecksBuilder AddCommonIdentityServerCheck(this IHealthChecksBuilder healthChecks, string identityServerUrl)
    {
        healthChecks.AddIdentityServer(
            idSvrUri: new(identityServerUrl),
            name: "identityServer",
            failureStatus: HealthStatus.Unhealthy);

        return healthChecks;
    }

    /// <summary>
    /// Provides an abstraction to adding a RabbitMQ check
    /// </summary>
    /// <param name="healthChecks">The health checks builder</param>
    /// <param name="connectionString">The amqp connection string for RabbitMQ</param>
    /// <returns>The health checks builder with the added health check</returns>
    public static IHealthChecksBuilder AddCommonRabbitMQCheck(
        this IHealthChecksBuilder healthChecks, string connectionString)
    {
        healthChecks.AddRabbitMQ(
            name: "rabbitmq",
            rabbitConnectionString: connectionString,
            failureStatus: HealthStatus.Degraded,
            timeout: TimeSpan.FromSeconds(1.5));

        return healthChecks;
    }

    /// <summary>
    /// Provides an abstraction to adding an Azure Service Bus topic check
    /// </summary>
    /// <param name="healthChecks">The health checks builder</param>
    /// <param name="connectionString">The connection string for Azure Service Bus retrieved from the access policy blade</param>
    /// <param name="topicName">The name of the topic</param>
    /// <returns>The health checks builder with the added health check</returns>
    public static IHealthChecksBuilder AddCommonAzureServiceBusTopicCheck(
        this IHealthChecksBuilder healthChecks, string connectionString, string topicName)
    {
        healthChecks.AddAzureServiceBusTopic(
            connectionString: connectionString,
            topicName: topicName,
            name: $"azureServiceBusTopic: {topicName}",
            failureStatus: HealthStatus.Degraded);

        return healthChecks;
    }

    /// <summary>
    /// Provides an abstraction to adding an Azure Service Bus topic subscription check
    /// </summary>
    /// <param name="healthChecks">The health checks builder</param>
    /// <param name="connectionString">The connection string for Azure Service Bus retrieved from the access policy blade</param>
    /// <param name="topicName">The name of the topic</param>
    /// <param name="subscriptionName">The name of the subscription under the topic</param>
    /// <returns>The health checks builder with the added health check</returns>
    public static IHealthChecksBuilder AddCommonAzureServiceBusSubscriptionCheck(
        this IHealthChecksBuilder healthChecks, string connectionString, string topicName, string subscriptionName)
    {
        healthChecks.AddAzureServiceBusSubscription(
            connectionString: connectionString,
            topicName: topicName,
            subscriptionName: subscriptionName,
            name: $"azureServiceBusTopicSubscription: {subscriptionName}",
            failureStatus: HealthStatus.Degraded);

        return healthChecks;
    }

    /// <summary>
    /// Maps the health and liveness health check endpoints for the web application
    /// </summary>
    /// <remarks>
    /// Uses the aspnetcore health checks ui client to write a formatted health check report
    /// </remarks>
    /// <param name="app">The web application used to configure the http pipeline</param>
    /// <returns>The web application used to configure the http pipeline with the mapped health check endpoints</returns>
    public static WebApplication MapCommonHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks(app.Configuration["HealthCheckEndpoint"]!, new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks(app.Configuration["LivenessCheckEndpoint"]!, new HealthCheckOptions
        {
            Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains("self"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
