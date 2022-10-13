using ConfigurationSubstitution;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SeelansTyres.Gateways.MvcBff.DelegatingHandlers;
using SeelansTyres.Gateways.MvcBff.Services;
using SeelansTyres.Libraries.Shared.Models;
using SeelansTyres.Libraries.Shared;
using static System.Net.Mime.MediaTypeNames;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Gateways.MvcBff.Extensions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration.GetValue<bool>("UseDocker") is false)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5050);
    });
}

builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

builder.Logging.ClearProviders();

var assembly = typeof(Program).Assembly;

var serilogModel = new SerilogModel
{
    Assembly = assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Mvc Backend-for-Frontend"
};

builder.Host.UseCommonSerilog(serilogModel);

var authenticationScheme = "SeelansTyresMvcBffAuthenticationScheme";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(authenticationScheme, configure =>
    {
        configure.Authority = builder.Configuration["IdentityServerUrl"];
        configure.Audience = "SeelansTyresMvcBff";
        configure.RequireHttpsMetadata = false;
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

var healthChecksModel = new HealthChecksModel
{
    EnableElasticsearchHealthCheck = builder.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled"),
    ElasticsearchUrl = builder.Configuration["LoggingSinks:Elasticsearch:Url"]
};

builder.Services.AddHealthChecks()
    .AddCommonChecks(healthChecksModel)
    .AddIdentityServer(
        idSvrUri: new(builder.Configuration["IdentityServerUrl"]),
        name: "identityServer",
        failureStatus: HealthStatus.Unhealthy)
    .AddDownstreamChecks(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();

await app.UseOcelot(ocelotPipelineConfiguration =>
{
    ocelotPipelineConfiguration.PreErrorResponderMiddleware = async (httpContext, next) =>
    {
        if (httpContext.Request.Path.Equals(new(app.Configuration["HealthCheckEndpoint"])) is true)
        {
            var healthCheckService = app.Services.GetService<HealthCheckService>();

            await UIResponseWriter.WriteHealthCheckUIResponse(httpContext, await healthCheckService!.CheckHealthAsync());
        }
        else
        {
            await next.Invoke();
        }
    };
});

app.Run();
