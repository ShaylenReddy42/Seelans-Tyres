using Microsoft.AspNetCore.Authentication.JwtBearer;   // JwtBearerDefaults
using Ocelot.DependencyInjection;                      // AddOcelot()
using Ocelot.Middleware;                               // UseOcelot()
using SeelansTyres.Gateways.MvcBff.DelegatingHandlers; // AddressServiceDelegatingHandler, CustomerServiceFullAccessDelegatingHandler, OrderServiceDelegatingHandler, TyresServiceDelegatingHandler
using SeelansTyres.Gateways.MvcBff.Services;           // ITokenExchangeService, TokenExchangeService
using static System.Net.Mime.MediaTypeNames;           // Application
using HealthChecks.UI.Client;                          // UIResponseWriter
using Microsoft.Extensions.Diagnostics.HealthChecks;   // HealthCheckService
using SeelansTyres.Gateways.MvcBff.Extensions;         // AddDownstreamChecks()
using SeelansTyres.Libraries.Shared.Extensions;        // AddCommonStartupDelay
using SeelansTyres.Libraries.Shared.Abstractions;      // All common methods

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Mvc Backend-for-Frontend"
});

// Against the norm, done for Ocelot
var authenticationScheme = "SeelansTyresMvcBffAuthenticationScheme";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(authenticationScheme, configure =>
    {
        configure.Authority = builder.Configuration["IdentityServer"];
        configure.Audience = "SeelansTyresMvcBff";
        configure.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddHttpClient<ITokenExchangeService, TokenExchangeService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["IdentityServer"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddCommonResiliencyPolicies<TokenExchangeService>(builder.Services);

builder.Services.AddScoped<AddressServiceDelegatingHandler>();
builder.Services.AddScoped<CustomerServiceFullAccessDelegatingHandler>();
builder.Services.AddScoped<OrderServiceDelegatingHandler>();
builder.Services.AddScoped<TyresServiceDelegatingHandler>();

builder.Services.AddOcelot()
    .AddDelegatingHandler<AddressServiceDelegatingHandler>()
    .AddDelegatingHandler<CustomerServiceFullAccessDelegatingHandler>()
    .AddDelegatingHandler<OrderServiceDelegatingHandler>()
    .AddDelegatingHandler<TyresServiceDelegatingHandler>();

builder.Services.AddHealthChecks()
    .AddCommonIdentityServerCheck(builder.Configuration["IdentityServer"]!)
    .AddDownstreamChecks(builder.Configuration);

var app = builder.Build();

app.HonorForwardedHeaders();

app.UseAuthentication();

await app.UseOcelot(ocelotPipelineConfiguration =>
{
    // Ocelot thinks the health checks are configured routes, which they aren't
    // 
    // Configure Ocelot in a way that matches the health check behavior of all applications
    // in the architecture by manually retrieving and invoking the health checks via the HealthCheckService
    // 
    // Allow the two health check routes 'HealthCheckEndpoint' and 'LivenessCheckEndpoint' from configuration
    // to be hit by catering for them
    // 
    // The regular app.MapHealthChecks() doesn't work with Ocelot and this is the workaround for that issue
    // 
    // The solution for this began with a comment on the same issue on Ocelot's repo
    // https://github.com/ThreeMammals/Ocelot/issues/646#issuecomment-425686026

    ocelotPipelineConfiguration.PreErrorResponderMiddleware = async (httpContext, next) =>
    {
        var requestPath = httpContext.Request.Path.ToString();
        var healthCheckService = app.Services.GetService<HealthCheckService>();

        var healthCheckEndpoints = new List<string>()
        {
            app.Configuration["HealthCheckEndpoint"]!,
            app.Configuration["LivenessCheckEndpoint"]!,
        };

        if (!healthCheckEndpoints.Contains(requestPath))
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

app.AddCommonStartupDelay();

app.Run();
