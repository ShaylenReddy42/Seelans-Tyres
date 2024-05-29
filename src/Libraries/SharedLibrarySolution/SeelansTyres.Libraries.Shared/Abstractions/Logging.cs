using Microsoft.ApplicationInsights.Extensibility; // TelemetryConfiguration
using Microsoft.Extensions.Configuration;          // GetValue()
using Microsoft.Extensions.DependencyInjection;    // GetRequiredService()
using Microsoft.Extensions.Hosting;                // IHostBuilder, Configuration
using SeelansTyres.Libraries.Shared.Configuration; // ElasticsearchLoggingSinkOptions, ExternalServiceOptions
using SeelansTyres.Libraries.Shared.Models;        // CommonBuilderConfigurationModel
using Serilog;                                     // UseSerilog()
using Serilog.Enrichers.Span;                      // ActivityEnricher
using Serilog.Events;                              // LogEventLevel
using Serilog.Exceptions;                          // WithExceptionDetails()
using Serilog.Settings.Configuration;              // ConfigurationReaderOptions
using Serilog.Sinks.Elasticsearch;                 // ElasticsearchSinkOptions, AutoRegisterTemplateVersion
using System.Reflection;                           // GetCustomAttribute(), AssemblyProductAttribute, AssemblyInformationalVersionAttribute, AssemblyMetadataAttribute

namespace SeelansTyres.Libraries.Shared.Abstractions;

public static class Logging
{
    /// <summary>
    /// Configures the default logging provider to Serilog
    /// </summary>
    /// <remarks>
    /// <para>
    ///     The logs are enriched with properties from the assembly<br/>
    ///     that make the logs more useful<br/>
    ///     <br/>
    ///     Those properties are:<br/>
    ///     1. The product name along with a descriptive version of it<br/>
    ///     2. The codebase version
    /// </para>
    /// <para>
    ///     The assembly may also contain custom metadata that is looped through<br/>
    ///     and used to enrich the logs<br/>
    ///     <br/>
    ///     Currently, only the Commit Url is there
    /// </para>
    /// <para>
    ///     The activity trace and span ids are also added via the 'ActivityEnricher' for distributed tracing
    /// </para>
    /// </remarks>
    /// <param name="hostBuilder">The web application's host builder</param>
    /// <param name="commonBuilderConfigurationModel">A model containing properties to enrich the logs and configure the Elasticsearch sink</param>
    /// <returns>The web application's host builder with Serilog as the logging provider</returns>
    public static IHostBuilder UseCommonSerilog(
        this IHostBuilder hostBuilder,
        CommonBuilderConfigurationModel commonBuilderConfigurationModel)
    {
        string descriptiveApplicationName =
            commonBuilderConfigurationModel.OriginAssembly
                .GetCustomAttribute<AssemblyProductAttribute>()?.Product
                    ?? commonBuilderConfigurationModel.DefaultDescriptiveApplicationName;

        string codebaseVersion =
            commonBuilderConfigurationModel.OriginAssembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? "0.0.0+0-unknown";

        var configurationAssemblies = new Assembly[]
        {
            typeof(Logging).Assembly
        };

        var serilogConfigurationReaderOptions = new ConfigurationReaderOptions(configurationAssemblies);

        hostBuilder.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostBuilderContext.Configuration, serilogConfigurationReaderOptions)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.With<ActivityEnricher>()
                .Enrich.WithProperty("Custom: Application Name", hostBuilderContext.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Custom: Descriptive Application Name", descriptiveApplicationName)
                .Enrich.WithProperty("Custom: Codebase Version", $"v{codebaseVersion}");

            loggerConfiguration
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level}] <{SourceContext}> {NewLine}{Message:lj} {NewLine}{Exception} {NewLine}");

            var metadata =
                commonBuilderConfigurationModel.OriginAssembly
                    .GetCustomAttributes<AssemblyMetadataAttribute>().ToList();

            metadata.ForEach(attribute => loggerConfiguration.Enrich.WithProperty($"Custom: {attribute.Key}", attribute.Value!));

            var elasticsearchLoggingSinkOptions =
                hostBuilderContext.Configuration.GetSection("LoggingSinks:Elasticsearch")
                    .Get<ElasticsearchLoggingSinkOptions>()
                        ?? throw new InvalidOperationException("Elasticsearch Logging Sink configuration section is missing");

            if (bool.Parse(elasticsearchLoggingSinkOptions.Enabled))
            {
                loggerConfiguration
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(elasticsearchLoggingSinkOptions.Url))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            IndexFormat = "seelanstyres-logs-{0:yyyy.MM.dd}",
                            MinimumLogEventLevel = LogEventLevel.Debug
                        });
            }

            var applicationInsightsOptions =
                hostBuilderContext.Configuration.GetSection("AppInsights")
                    .Get<ExternalServiceOptions>()
                        ?? throw new InvalidOperationException("AppInsights configuration section is missing");

            if (applicationInsightsOptions.Enabled)
            {
                loggerConfiguration
                    .WriteTo.ApplicationInsights(
                        serviceProvider.GetRequiredService<TelemetryConfiguration>(),
                        TelemetryConverter.Events);
            }
        });

        return hostBuilder;
    }
}