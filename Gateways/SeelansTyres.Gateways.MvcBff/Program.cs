using ConfigurationSubstitution;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SeelansTyres.Gateways.MvcBff.DelegatingHandlers;
using SeelansTyres.Gateways.MvcBff.Services;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

builder.Logging.ClearProviders();

var assembly = typeof(Program).Assembly;

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostBuilderContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("Application Name", hostBuilderContext.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Descriptive Application Name", assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product)
        .Enrich.WithProperty("Codebase Version", $"v{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}")
        .WriteTo.Console();

    var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();

    metadata.ForEach(attribute => loggerConfiguration.Enrich.WithProperty(attribute.Key, attribute.Value));

    if (hostBuilderContext.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled") is true)
    {
        loggerConfiguration
            .WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(hostBuilderContext.Configuration["LoggingSinks:Elasticsearch:Url"]))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = "seelanstyres-logs-{0:yyyy.MM.dd}",
                    MinimumLogEventLevel = LogEventLevel.Debug
                });
    }
});

var authenticationScheme = "SeelansTyresMvcBffAuthenticationScheme";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(authenticationScheme, configure =>
    {
        configure.Authority = builder.Configuration["IdentityServerUrl"];
        configure.Audience = "SeelansTyresMvcBff";
    });

builder.Services.AddHttpClient<ITokenExchangeService, TokenExchangeService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["IdentityServerUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddScoped<AddressServiceDelegatingHandler>();
builder.Services.AddScoped<CustomerServiceFullAccessDelegatingHandler>();
builder.Services.AddScoped<OrderServiceDelegatingHandler>();
builder.Services.AddScoped<TyresServiceDelegatingHandler>();

builder.Services.AddOcelot()
    .AddDelegatingHandler<AddressServiceDelegatingHandler>()
    .AddDelegatingHandler<CustomerServiceFullAccessDelegatingHandler>()
    .AddDelegatingHandler<OrderServiceDelegatingHandler>()
    .AddDelegatingHandler<TyresServiceDelegatingHandler>();

var app = builder.Build();

app.Urls.Clear();
app.Urls.Add("https://localhost:5050");
app.Urls.Add("http://localhost:4050");

app.UseAuthentication();

await app.UseOcelot();

app.Run();
