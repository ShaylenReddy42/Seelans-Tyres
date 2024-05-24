using Microsoft.AspNetCore.Authentication.JwtBearer;   // JwtBearerDefaults
using Ocelot.DependencyInjection;                      // AddOcelot()
using Ocelot.Middleware;                               // UseOcelot()
using SeelansTyres.Gateways.MvcBff.DelegatingHandlers; // AddressServiceDelegatingHandler, CustomerServiceFullAccessDelegatingHandler, OrderServiceDelegatingHandler, TyresServiceDelegatingHandler
using SeelansTyres.Gateways.MvcBff.Services;           // ITokenExchangeService, TokenExchangeService
using static System.Net.Mime.MediaTypeNames;           // Application
using HealthChecks.UI.Client;                          // UIResponseWriter
using SeelansTyres.Gateways.MvcBff.Extensions;         // AddDownstreamChecks()
using SeelansTyres.Libraries.Shared.Extensions;        // AddCommonStartupDelay
using SeelansTyres.Libraries.Shared.Abstractions;      // All common methods
using Microsoft.AspNetCore.Diagnostics.HealthChecks;   // HealthCheckOptions

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
        configure.TokenValidationParameters.ValidTypes = [ "at+jwt" ];
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

await app.UseOcelot();

app.MapHealthChecks(app.Configuration["HealthCheckEndpoint"]!, new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).ShortCircuit();

app.MapHealthChecks(app.Configuration["LivenessCheckEndpoint"]!, new HealthCheckOptions
{
    Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains("self"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).ShortCircuit();

app.AddCommonStartupDelay();

await app.RunAsync();
