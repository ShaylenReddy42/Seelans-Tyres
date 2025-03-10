﻿using HealthChecks.UI.Client;                        // UIResponseWriter
using Microsoft.AspNetCore.Builder;                  // WebApplication
using Microsoft.AspNetCore.Diagnostics.HealthChecks; // HealthCheckOptions
using Microsoft.EntityFrameworkCore;                 // DbContext, AddDbContextCheck()
using Microsoft.Extensions.Configuration;            // Get()
using Microsoft.Extensions.DependencyInjection;      // IHealthChecksBuilder
using Microsoft.Extensions.Diagnostics.HealthChecks; // HealthCheckResult, HealthStatus
using SeelansTyres.Libraries.Shared.Configuration;   // ElasticsearchLoggingSinkOptions

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
    /// <param name="elasticsearchLoggingSinkOptions">Elasticsearch options coming from a POCO bound to configuration</param>
    /// <param name="publishHealthStatusToAppicationInsights">Enable publishing the health check status to Application Insights</param>
    /// <returns>The original health checks builder with a check on self and Elasticsearch if enabled</returns>
    public static IHealthChecksBuilder AddCommonChecks(
        this IHealthChecksBuilder healthChecks, 
        ElasticsearchLoggingSinkOptions elasticsearchLoggingSinkOptions,
        bool publishHealthStatusToAppicationInsights)
    {
        healthChecks
            .AddCheck(
                name: "Self",
                check: () => HealthCheckResult.Healthy());

        if (bool.Parse(elasticsearchLoggingSinkOptions.Enabled))
        {
            healthChecks
                .AddElasticsearch(
                    elasticsearchUri: elasticsearchLoggingSinkOptions.Url,
                    name: "Elasticsearch",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(1.5));
        }

        if (publishHealthStatusToAppicationInsights)
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
            name: "Database",
            failureStatus: HealthStatus.Unhealthy);

        return healthChecks;
    }

    /// <summary>
    /// Provides an abstraction to adding an identity server check
    /// </summary>
    /// <param name="healthChecks">The health checks builder</param>
    /// <param name="identityServerUrl">The url of the identity server</param>
    /// <returns>The health checks builder with the added health check</returns>
    public static IHealthChecksBuilder AddCommonOidcServerCheck(this IHealthChecksBuilder healthChecks, string identityServerUrl)
    {
        healthChecks.AddOpenIdConnectServer(
            oidcSvrUri: new(identityServerUrl),
            name: "Identity Server",
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
            name: "RabbitMQ",
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
            name: $"Azure Service Bus Topic: {topicName}",
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
            name: $"Azure Service Bus Topic Subscription: {subscriptionName}",
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
        var healthCheckEndpointOptions =
            app.Configuration
                .Get<HealthCheckEndpointOptions>()
                    ?? throw new InvalidOperationException("HealthCheckEndpoint and LivenessCheckEndpoint settings are missing in configuration");

        app.MapHealthChecks(healthCheckEndpointOptions.HealthCheckEndpoint, new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks(healthCheckEndpointOptions.LivenessCheckEndpoint, new HealthCheckOptions
        {
            Predicate = healthCheckRegistration => healthCheckRegistration.Name is "Self",
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
