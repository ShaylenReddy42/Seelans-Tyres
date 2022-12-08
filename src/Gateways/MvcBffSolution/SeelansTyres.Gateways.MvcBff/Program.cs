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

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5050,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Mvc Backend-for-Frontend"
});

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
    client.BaseAddress = new Uri(builder.Configuration["IdentityServerUrl"]!);
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
    ElasticsearchUrl = builder.Configuration["LoggingSinks:Elasticsearch:Url"]!
};

builder.Services.AddHealthChecks()
    .AddCommonChecks(healthChecksModel)
    .AddIdentityServer(
        idSvrUri: new(builder.Configuration["IdentityServerUrl"]!),
        name: "identityServer",
        failureStatus: HealthStatus.Unhealthy)
    .AddDownstreamChecks(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();

await app.UseOcelot(ocelotPipelineConfiguration =>
{
    ocelotPipelineConfiguration.PreErrorResponderMiddleware = async (httpContext, next) =>
    {
        var requestPath = httpContext.Request.Path.ToString();
        var healthCheckService = app.Services.GetService<HealthCheckService>();

        var healthCheckEndpoints = new List<string>()
        {
            app.Configuration["HealthCheckEndpoint"]!,
            app.Configuration["LivenessCheckEndpoint"]!,
        };

        if (healthCheckEndpoints.Contains(requestPath) is false)
        {
            await next.Invoke();
        }
        else if (requestPath == app.Configuration["LivenessCheckEndpoint"])
        {
            await UIResponseWriter.WriteHealthCheckUIResponse(
                httpContext, 
                await healthCheckService!.CheckHealthAsync(
                    healthCheckRegistration =>  
                        healthCheckRegistration.Tags.Contains("self")));
        }
        else if (requestPath == app.Configuration["HealthCheckEndpoint"])
        {
            await UIResponseWriter.WriteHealthCheckUIResponse(
                httpContext, 
                await healthCheckService!.CheckHealthAsync());
        }
    };
});

app.Run();
