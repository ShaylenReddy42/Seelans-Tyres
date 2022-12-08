using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SeelansTyres.Libraries.Shared.Models;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

namespace SeelansTyres.Libraries.Shared;

public static class Logging
{
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

        hostBuilder.UseSerilog((hostBuilderContext, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostBuilderContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.With<ActivityEnricher>()
                .Enrich.WithProperty("Custom: Application Name", hostBuilderContext.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Custom: Descriptive Application Name", descriptiveApplicationName)
                .Enrich.WithProperty("Custom: Codebase Version", $"v{codebaseVersion}");
            
            if (hostBuilderContext.HostingEnvironment.ApplicationName.EndsWith("IdentityService") is true)
            {
                loggerConfiguration
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", 
                        theme: AnsiConsoleTheme.Code);
            }
            else
            {
                loggerConfiguration
                    .WriteTo.Console();
            }

            var metadata =
                commonBuilderConfigurationModel.OriginAssembly
                    .GetCustomAttributes<AssemblyMetadataAttribute>().ToList();

            metadata.ForEach(attribute => loggerConfiguration.Enrich.WithProperty($"Custom: {attribute.Key}", attribute.Value));

            if (hostBuilderContext.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled") is true)
            {
                loggerConfiguration
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(hostBuilderContext.Configuration["LoggingSinks:Elasticsearch:Url"]!))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            IndexFormat = "seelanstyres-logs-{0:yyyy.MM.dd}",
                            MinimumLogEventLevel = LogEventLevel.Debug
                        });
            }
        });

        return hostBuilder;
    }
}